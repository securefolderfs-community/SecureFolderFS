using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
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
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Shared.SecureStore;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.Scanners;
using static SecureFolderFS.Core.Cryptography.Constants;

namespace SecureFolderFS.Core.Routines.Operational
{
    /// <inheritdoc cref="ICredentialsRoutine"/>
    public sealed class RestoreRoutine : ICredentialsRoutine, IFinalizationRoutine
    {

        private readonly IFolder _vaultFolder;
        private readonly VaultWriter _vaultWriter;
        private readonly Dictionary<string, IContentCrypt> _contentCrypts;
        private readonly Dictionary<string, IHeaderCrypt> _headerCrypts;
        private List<INameCrypt>? _nameCrypts;
        private VaultKeystoreDataModel? _keystoreDataModel;
        private VaultConfigurationDataModel? _configDataModel;
        private KeyPair? _keyPair;
        private VaultRestorationParameters? _detectedParameters;
        private bool _parametersConfirmed;

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

            // The configuration written here is signed with the vault's genuine MAC key and is therefore
            // indistinguishable from one the user created. It must never be produced from parameters the
            // user has not seen and accepted
            var parameters = await DetectParametersAsync(cancellationToken);
            if (!_parametersConfirmed)
                throw new InvalidOperationException("The detected vault parameters must be confirmed before the configuration can be rebuilt.");

            // Regenerate config
            var configDataModel = new VaultConfigurationDataModel()
            {
                AppPlatform = null,
                AuthenticationMethod = Constants.Vault.Authentication.AUTH_RECOVERY_KEY_REQUIREMENT, // Recovery Key is required at first to recover the restored vault
                ContentCipherId = parameters.ContentCipherId,
                FileNameCipherId = parameters.FileNameCipherId,
                FileNameEncodingId = parameters.FileNameEncodingId,
                ShorteningThreshold = parameters.ShorteningThreshold,
                RecycleBinSize = 0L,
                Uid = Guid.NewGuid().ToString(),
                Version = Constants.Vault.Versions.LATEST_VERSION,
                PayloadMac = new byte[HMACSHA256.HashSizeInBytes]
            };

            // Calculate config MAC
            _keyPair.MacKey.UseKey(macKey =>
            {
                VaultParser.CalculateConfigMac(configDataModel, macKey, configDataModel.PayloadMac);
            });

            // Regenerate keystore
            var keystore = GenerateKeystore(_keyPair);

            // Write the whole configuration
            await _vaultWriter.WriteConfigurationAsync(configDataModel, cancellationToken);
            await _vaultWriter.WriteKeystoreAsync(keystore, cancellationToken);

            return new SecurityWrapper(_keyPair.CreateCopy(), configDataModel);
        }

        /// <summary>
        /// Determines the cryptographic parameters of the vault by probing its contents.
        /// </summary>
        /// <remarks>
        /// The result must be presented to the user and confirmed through <see cref="ConfirmParameters"/>
        /// before <see cref="FinalizeAsync"/> will rebuild the configuration.
        /// </remarks>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the detected parameters.</returns>
        public async Task<VaultRestorationParameters> DetectParametersAsync(CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(_keyPair);

            if (_detectedParameters is not null)
                return _detectedParameters;

            var contentFolder = await _vaultFolder.GetFolderByNameAsync(Constants.Vault.Names.VAULT_CONTENT_FOLDERNAME, cancellationToken);
            var contentCryptIds = new[] { CipherId.AES_GCM, CipherId.XCHACHA20_POLY1305, CipherId.AES_CTR_HMAC };

            string? foundContentCrypt = null;
            string? foundNameCrypt = null;
            string? foundEncoding = null;
            var minSidecarContentLength = int.MaxValue;
            var hasShortenedNames = false;

            var folderScanner = new DeepFolderScanner(contentFolder, StorableType.All);
            await foreach (var item in folderScanner.ScanFolderAsync(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Shortened items are hashes that can't be decrypted directly
                if (item.Name.EndsWith(FileSystem.Constants.Names.SHORTENED_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase))
                {
                    hasShortenedNames = true;
                    continue;
                }

                if (item is not IFile file)
                    continue;

                // Read sidecar files to determine the shortening threshold from the actual ciphertext name length
                if (item.Name.EndsWith(FileSystem.Constants.Names.SIDECAR_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await using var sidecarStream = await file.OpenReadAsync(cancellationToken);
                        var buffer = new byte[4097];
                        var bytesRead = await sidecarStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                        if (bytesRead is > 0 and <= 4096)
                            minSidecarContentLength = Math.Min(minSidecarContentLength, Encoding.UTF8.GetString(buffer, 0, bytesRead).Length);
                    }
                    catch
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    continue;
                }

                // Vault infrastructure never carries an encrypted name, so it must take no part in the
                // probe below. Treating a dirid.iv as a name that failed to decrypt is what allowed a
                // vault with a handful of directories to conclude, with no attacker at all, that it
                // had no filename encryption
                if (PathHelpers.IsCoreName(item.Name))
                    continue;

                // Find content crypt
                foundContentCrypt ??= await FindContentCryptAsync(file, _keyPair, contentCryptIds, cancellationToken);

                // Find name crypt. The cipher is never inferred from what a name looks like.
                // It is established only by an authenticated decryption that succeeds, so planting files cannot steer the result
                if (foundNameCrypt is null || foundEncoding is null)
                    (foundNameCrypt, foundEncoding) = await FindNameCryptAsync(contentFolder, file, _keyPair, cancellationToken);

                // We have found everything
                if (foundNameCrypt is not null && foundEncoding is not null && foundContentCrypt is not null)
                    break;
            }

            // The content cipher is always established by trial decryption, so failing to find one
            // means the vault holds nothing this routine can authenticate and the restore cannot proceed
            if (foundContentCrypt is null)
                throw new InvalidOperationException("Could not determine the content cipher of the vault.");

            // Every candidate name was probed with AES-SIV across both encodings and none authenticated.
            // Having positively ruled out filename encryption, the names are stored in clear
            if (foundNameCrypt is null || foundEncoding is null)
                (foundNameCrypt, foundEncoding) = (CipherId.NONE, CipherId.ENCODING_BASE4K);

            // Determine shortening threshold from sidecar content, with fallback for missing sidecars
            var shorteningThreshold = minSidecarContentLength < int.MaxValue
                ? minSidecarContentLength
                : hasShortenedNames ? 220 : 0;

            _detectedParameters = new VaultRestorationParameters()
            {
                ContentCipherId = foundContentCrypt,
                FileNameCipherId = foundNameCrypt,
                FileNameEncodingId = foundEncoding,
                ShorteningThreshold = shorteningThreshold,
                IsFileNameEncrypted = !string.Equals(foundNameCrypt, CipherId.NONE, StringComparison.Ordinal)
            };

            return _detectedParameters;
        }

        /// <summary>
        /// Accepts the parameters returned by <see cref="DetectParametersAsync"/>, allowing the
        /// configuration to be rebuilt from them.
        /// </summary>
        public void ConfirmParameters()
        {
            if (_detectedParameters is null)
                throw new InvalidOperationException($"{nameof(DetectParametersAsync)} must be called before the parameters can be confirmed.");

            _parametersConfirmed = true;
        }

        private unsafe VaultKeystoreDataModel GenerateKeystore(KeyPair keyPair)
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
                        return VaultParser.EncryptKeystore(pK, dekKey, macKey, salt);
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