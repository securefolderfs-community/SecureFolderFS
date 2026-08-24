using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Api.Helpers;
using SecureFolderFS.Sdk.Api.Models;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.Api.Services
{
    /// <inheritdoc cref="IPairingStore"/>
    public sealed class PairingStore : IPairingStore
    {
        private readonly string _fileName;
        private readonly IModifiableFolder _settingsFolder;
        private readonly IAsyncSerializer<Stream> _serializer;
        private readonly Lock _lock = new();
        private readonly SemaphoreSlim _fileLock = new(1, 1);

        private PairingStoreDataModel _data = new();
        private IFile? _file;
        private bool _initialized;

        /// <inheritdoc/>
        public TimeSpan DenialCooldown { get; } = TimeSpan.FromMinutes(5);

        public PairingStore(string fileName, IModifiableFolder settingsFolder, IAsyncSerializer<Stream> serializer)
        {
            _fileName = fileName;
            _settingsFolder = settingsFolder;
            _serializer = serializer;
        }

        /// <inheritdoc/>
        public byte[] VaultIdKey
        {
            get
            {
                lock (_lock)
                {
                    EnsureInitialized();
                    return Convert.FromBase64String(_data.VaultIdKey);
                }
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<PairedClient> GetClients()
        {
            lock (_lock)
                return _data.Clients.ToArray();
        }

        /// <inheritdoc/>
        public PairedClient? Authenticate(string? token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var candidateHash = ComputeTokenHash(token);
            lock (_lock)
            {
                // Compared in constant time so a caller cannot learn a valid digest by timing rejections
                return _data.Clients.FirstOrDefault(x => FixedTimeEqualsBase64(x.TokenHash, candidateHash));
            }
        }

        /// <inheritdoc/>
        public bool IsInDenialCooldown(string fingerprint)
        {
            lock (_lock)
            {
                var entry = _data.Denied.Find(x => x.Fingerprint == fingerprint);
                if (entry is null)
                    return false;

                if (DateTimeOffset.UtcNow - entry.DeniedAt < DenialCooldown)
                    return true;

                // Expired: drop it from memory; the file is reconciled on the next save
                _data.Denied.Remove(entry);
                return false;
            }
        }

        /// <inheritdoc/>
        public Task TouchLastUsedAsync(string clientId, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                EnsureInitialized();

                var index = _data.Clients.FindIndex(x => x.Id == clientId);
                if (index < 0)
                    return Task.CompletedTask;

                _data.Clients[index] = _data.Clients[index] with { LastUsedAt = DateTimeOffset.UtcNow };
            }

            return SaveAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<string> CreateClientAsync(
            string displayName, PeerEvidence evidence, IReadOnlyList<string> scopes, CancellationToken cancellationToken = default)
        {
            var token = PublicVaultIdHelpers.ToBase64Url(RandomNumberGenerator.GetBytes(32));
            var client = new PairedClient(
                Id: Guid.NewGuid().ToString("N"),
                DisplayName: displayName,
                TokenHash: ComputeTokenHash(token),
                WasIdentityVerified: evidence.IsVerified,
                ExecutablePath: evidence.ExecutablePath,
                Signer: evidence.Signer,
                Scopes: scopes,
                CreatedAt: DateTimeOffset.UtcNow,
                LastUsedAt: null);

            lock (_lock)
            {
                EnsureInitialized();
                _data.Clients.Add(client);
            }

            await SaveAsync(cancellationToken);
            return token;
        }

        /// <inheritdoc/>
        public Task RevokeClientAsync(string clientId, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                EnsureInitialized();
                if (_data.Clients.RemoveAll(x => x.Id == clientId) == 0)
                    return Task.CompletedTask;
            }

            return SaveAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public Task RevokeAllAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                EnsureInitialized();
                if (_data.Clients.Count == 0 && _data.Denied.Count == 0)
                    return Task.CompletedTask;

                _data.Clients.Clear();
                _data.Denied.Clear();
            }

            return SaveAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public Task RecordDenialAsync(string fingerprint, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                EnsureInitialized();
                _data.Denied.RemoveAll(x => x.Fingerprint == fingerprint);
                _data.Denied.Add(new DeniedClient(fingerprint, DateTimeOffset.UtcNow));
            }

            return SaveAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await _fileLock.WaitAsync(cancellationToken);
            try
            {
                if (_initialized)
                    return;

                _file ??= await _settingsFolder.CreateFileAsync(_fileName, false, cancellationToken);

                var loaded = await TryReadAsync(_file, cancellationToken);
                if (loaded is not null && !string.IsNullOrEmpty(loaded.VaultIdKey))
                {
                    lock (_lock)
                        _data = loaded;
                }
                else
                {
                    // A missing or corrupt store is replaced which revokes every pairing
                    PairingStoreDataModel fresh;
                    lock (_lock)
                        fresh = _data = new PairingStoreDataModel { VaultIdKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) };

                    await WriteAsync(_file, fresh, cancellationToken);
                }

                _initialized = true;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await _fileLock.WaitAsync(cancellationToken);
            try
            {
                _file ??= await _settingsFolder.CreateFileAsync(_fileName, false, cancellationToken);

                PairingStoreDataModel snapshot;
                lock (_lock)
                    snapshot = _data.Clone();

                await WriteAsync(_file, snapshot, cancellationToken);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private async Task<PairingStoreDataModel?> TryReadAsync(IFile file, CancellationToken cancellationToken)
        {
            try
            {
                await using var stream = await file.OpenReadAsync(cancellationToken);
                return await _serializer.DeserializeAsync<Stream, PairingStoreDataModel>(stream, cancellationToken);
            }
            catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
            {
                return null;
            }
        }

        private async Task WriteAsync(IFile file, PairingStoreDataModel data, CancellationToken cancellationToken)
        {
            await using var destination = await file.OpenWriteAsync(cancellationToken);
            await using var serialized = await _serializer.SerializeAsync<Stream, PairingStoreDataModel>(data, cancellationToken);

            destination.SetLength(0L);
            serialized.Position = 0L;
            await serialized.CopyToAsync(destination, cancellationToken);
            await destination.FlushAsync(cancellationToken);
        }

        private static string ComputeTokenHash(string token)
            => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        private static bool FixedTimeEqualsBase64(string left, string right)
        {
            try
            {
                return CryptographicOperations.FixedTimeEquals(
                    Convert.FromBase64String(left),
                    Convert.FromBase64String(right));
            }
            catch (FormatException)
            {
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureInitialized()
        {
            if (!_initialized)
                throw new InvalidOperationException($"{nameof(PairingStore)} must be initialized before use.");
        }
    }
}
