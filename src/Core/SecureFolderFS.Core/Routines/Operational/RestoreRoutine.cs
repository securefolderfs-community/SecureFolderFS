using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.ContentCrypt;
using SecureFolderFS.Core.Cryptography.HeaderCrypt;
using SecureFolderFS.Core.Cryptography.NameCrypt;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.FileSystem.Buffers;
using SecureFolderFS.Core.FileSystem.Extensions;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.SecureStore;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.Scanners;
using static SecureFolderFS.Core.Cryptography.Constants;

namespace SecureFolderFS.Core.Routines.Operational
{
    /// <inheritdoc cref="ICredentialsRoutine"/>
    public sealed class RestoreRoutine : ICredentialsRoutine, IFinalizationRoutine
    {
        private const int NO_EXTENSIONS_THRESHOLD = 5;

        private readonly IFolder _vaultFolder;
        private readonly VaultWriter _vaultWriter;
        private readonly Dictionary<string, IContentCrypt> _contentCrypts;
        private readonly Dictionary<string, IHeaderCrypt> _headerCrypts;
        private List<INameCrypt>? _nameCrypts;
        private V4VaultKeystoreDataModel? _keystoreDataModel;
        private V4VaultConfigurationDataModel? _configDataModel;
        private KeyPair? _keyPair;

        public RestoreRoutine(IFolder vaultFolder, VaultWriter vaultWriter)
        {
            _headerCrypts = new();
            _contentCrypts = new();
            _vaultFolder = vaultFolder;
            _vaultWriter = vaultWriter;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void SetCredentials(IKeyUsage passkey)
        {
            _keyPair = KeyPair.CopyFromRecoveryKey(passkey);
        }

        /// <inheritdoc/>
        public async Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_keyPair);

            var contentFolder = await _vaultFolder.GetFolderByNameAsync(Constants.Vault.Names.VAULT_CONTENT_FOLDERNAME, cancellationToken);
            var contentCryptIds = new[] { CipherId.AES_GCM, CipherId.XCHACHA20_POLY1305, CipherId.AES_CTR_HMAC };

            string? foundContentCrypt = null;
            string? foundNameCrypt = null;
            string? foundEncoding = null;
            var noExtensions = 0;

            var folderScanner = new DeepFolderScanner(contentFolder, StorableType.File);
            await foreach (var item in folderScanner.ScanFolderAsync(cancellationToken))
            {
                if (item is not IFile file)
                    continue;

                if (!item.Name.EndsWith(FileSystem.Constants.Names.ENCRYPTED_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase))
                    noExtensions++;

                // Check if we have enough files without extensions to be certain
                if (noExtensions >= NO_EXTENSIONS_THRESHOLD)
                    (foundNameCrypt, foundEncoding) = (CipherId.NONE, CipherId.ENCODING_BASE4K);

                // Find content crypt
                foundContentCrypt ??= await FindContentCryptAsync(file, _keyPair, contentCryptIds, cancellationToken);

                // Find name crypt
                if (foundNameCrypt is null || foundEncoding is null)
                    (foundNameCrypt, foundEncoding) = await FindNameCryptAsync(contentFolder, file, _keyPair, cancellationToken);

                // We have found everything
                if (foundNameCrypt is not null && foundEncoding is not null && foundContentCrypt is not null)
                    break;
            }

            if (foundNameCrypt is null || foundEncoding is null || foundContentCrypt is null)
                throw new InvalidOperationException("Could not find all required cryptographic components.");

            // Regenerate config
            var configDataModel = new V4VaultConfigurationDataModel()
            {
                AppPlatform = null,
                AuthenticationMethod = Constants.Vault.Authentication.AUTH_RECOVERY_KEY_REQUIREMENT, // Recovery Key is required at first to recover the restored vault
                ContentCipherId = foundContentCrypt,
                FileNameCipherId = foundNameCrypt,
                FileNameEncodingId = foundEncoding,
                RecycleBinSize = 0L,
                Uid = Guid.NewGuid().ToString(),
                Version = Constants.Vault.Versions.LATEST_VERSION,
                PayloadMac = new byte[HMACSHA256.HashSizeInBytes]
            };

            // Calculate config MAC
            _keyPair.MacKey.UseKey(macKey =>
            {
                VaultParser.V4CalculateConfigMac(configDataModel, macKey, configDataModel.PayloadMac);
            });

            // Regenerate keystore
            var keystore = GenerateKeystore(_keyPair);

            // Write the whole configuration
            await _vaultWriter.WriteV4ConfigurationAsync(configDataModel, cancellationToken);
            await _vaultWriter.WriteKeystoreAsync(keystore, cancellationToken);

            return new SecurityWrapper(_keyPair.CreateCopy(), configDataModel);
        }

        private unsafe V4VaultKeystoreDataModel GenerateKeystore(KeyPair keyPair)
        {
            // A strong, random passkey is generated because the user will
            // be responsible later for resetting credentials
            using var strongPasskey = SecureKey.CreateSecureRandom(KeyTraits.KEY_PART_LENGTH_128);
            var salt = RandomNumberGenerator.GetBytes(KeyTraits.SALT_LENGTH);

            return strongPasskey.UseKey(passkey =>
            {
                fixed (byte* passkeyPtr = passkey)
                {
                    var state = (pPtr: (nint)passkeyPtr, pLen: passkey.Length);
                    return keyPair.UseKeys(state, (dekKey, macKey, s) =>
                    {
                        var pK = new ReadOnlySpan<byte>((byte*)s.pPtr, s.pLen);
                        return VaultParser.V4EncryptKeystore(pK, dekKey, macKey, salt);
                    });
                }
            });
        }

        private async Task<(string? foundNameCrypt, string? foundEncoding)> FindNameCryptAsync(IFolder contentFolder, IFile file, KeyPair keyPair, CancellationToken cancellationToken)
        {
            var expendableDirectoryId = new byte[FileSystem.Constants.DIRECTORY_ID_SIZE];
            _nameCrypts ??=
            [
                Security.GetNameCrypt(keyPair, CipherId.AES_SIV, CipherId.ENCODING_BASE4K),
                Security.GetNameCrypt(keyPair, CipherId.AES_SIV, CipherId.ENCODING_BASE64URL)
            ];
            foreach (var nameCrypt in _nameCrypts)
            {
                try
                {
                    if (file is not IChildFile childFile)
                        continue;

                    var parentFolder = await childFile.GetParentAsync(cancellationToken);
                    if (parentFolder is null)
                        continue;

                    var result = await AbstractPathHelpers.GetDirectoryIdAsync(parentFolder, contentFolder, expendableDirectoryId, cancellationToken);
                    var normalizedName = AbstractPathHelpers.RemoveCiphertextExtension(file.Name);
                    var decryptedName = nameCrypt.DecryptName(normalizedName, result ? expendableDirectoryId : ReadOnlySpan<byte>.Empty);

                    if (string.IsNullOrEmpty(decryptedName))
                        continue;

                    return (CipherId.AES_SIV, nameCrypt.EncodingId); // TODO: There's only one supported cipher, so a constant value is used
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            return (null, null);
        }

        private async Task<string?> FindContentCryptAsync(IFile file, KeyPair keyPair, string[] contentCryptIds, CancellationToken cancellationToken)
        {
            await using var stream = await file.OpenReadAsync(cancellationToken);
            foreach (var item in contentCryptIds)
            {
                stream.Position = 0L;
                if (!_contentCrypts.TryGetValue(item, out var contentCrypt))
                {
                    contentCrypt = Security.GetContentCrypt(item, keyPair);
                    _contentCrypts[item] = contentCrypt;
                }

                if (!_headerCrypts.TryGetValue(item, out var headerCrypt))
                {
                    headerCrypt = Security.GetHeaderCrypt(keyPair, item);
                    _headerCrypts[item] = headerCrypt;
                }

                var headerBuffer = new HeaderBuffer(headerCrypt.HeaderPlaintextSize);
                if (!headerBuffer.ReadHeader(stream, headerCrypt) || !headerBuffer.IsHeaderReady)
                    continue;

                var ciphertextChunk = new byte[contentCrypt.ChunkCiphertextSize];
                stream.Position = headerCrypt.HeaderCiphertextSize;

                var read = await stream.ReadAsync(ciphertextChunk, cancellationToken);
                var plaintextChunk = new byte[contentCrypt.ChunkPlaintextSize];

                if (contentCrypt.DecryptChunk(ciphertextChunk.AsSpan(0, read), 0, headerBuffer, plaintextChunk))
                    return item;
            }

            return null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _headerCrypts.Values.DisposeAll();
            _contentCrypts.Values.DisposeAll();
            _nameCrypts?.DisposeAll();
            _keyPair?.Dispose();
        }
    }
}