using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Routines.CreationRoutines
{
    /// <inheritdoc cref="ICreationRoutine"/>
    internal sealed class CreationRoutine : ICreationRoutine
    {
        private readonly CipherProvider _cipherProvider;
        private VaultKeystoreDataModel? _keystoreDataModel;
        private SecretKey? _macKey;

        public CreationRoutine()
        {
            _cipherProvider = CipherProvider.CreateNew();
        }

        /// <inheritdoc/>
        public async Task CreateContentFolderAsync(IModifiableFolder vaultFolder, CancellationToken cancellationToken = default)
        {
            _ = await vaultFolder.CreateFolderAsync(Constants.CONTENT_FOLDERNAME, CreationCollisionOption.FailIfExists, cancellationToken);
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public void SetVaultPassword(IPassword password)
        {
            using (password)
            {
                using var encKey = new SecureKey(new byte[Constants.KeyChains.ENCRYPTIONKEY_LENGTH]);
                using var macKey = new SecureKey(new byte[Constants.KeyChains.MACKEY_LENGTH]);
                var salt = new byte[Constants.KeyChains.SALT_LENGTH];

                // Fill keys
                using var secureRandom = RandomNumberGenerator.Create();
                secureRandom.GetNonZeroBytes(encKey.Key);
                secureRandom.GetNonZeroBytes(macKey.Key);
                secureRandom.GetNonZeroBytes(salt);

                // Derive KEK
                Span<byte> kek = stackalloc byte[Cryptography.Constants.ARGON2_KEK_LENGTH];
                _cipherProvider.Argon2idCrypt.DeriveKey(password.GetPassword(), salt, kek);

                // Wrap keys
                var wrappedEncKey = _cipherProvider.Rfc3394KeyWrap.WrapKey(encKey, kek);
                var wrappedMacKey = _cipherProvider.Rfc3394KeyWrap.WrapKey(macKey, kek);

                // Construct keystore data model
                _keystoreDataModel = new()
                {
                    WrappedEncKey = wrappedEncKey,
                    WrappedMacKey = wrappedMacKey,
                    Salt = salt
                };

                // Create MAC key copy for later use
                _macKey = macKey.CreateCopy();
            }
        }

        /// <inheritdoc/>
        public async Task WriteKeystoreAsync(Stream keystoreStream, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken = default)
        {
            await using var serializedKeystoreStream = await serializer.SerializeAsync(_keystoreDataModel, cancellationToken);
            await serializedKeystoreStream.CopyToAsync(keystoreStream, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task WriteConfigurationAsync(VaultOptions vaultOptions, Stream configStream, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(_macKey);

            using (_macKey)
            {
                using var hmacSha256Crypt = _cipherProvider.HmacSha256Crypt.GetInstance();
                hmacSha256Crypt.InitializeHmac(_macKey);
                hmacSha256Crypt.Update(BitConverter.GetBytes(Constants.VaultVersion.LATEST_VERSION));
                hmacSha256Crypt.Update(BitConverter.GetBytes((uint)vaultOptions.FileNameCipherScheme));
                hmacSha256Crypt.Update(BitConverter.GetBytes((uint)vaultOptions.ContentCipherScheme));

                var payloadMac = new byte[_cipherProvider.HmacSha256Crypt.MacSize];
                hmacSha256Crypt.GetHash(payloadMac);

                var configurationDataModel = new VaultConfigurationDataModel()
                {
                    ContentCipherScheme = vaultOptions.ContentCipherScheme,
                    FileNameCipherScheme = vaultOptions.FileNameCipherScheme,
                    PayloadMac = payloadMac
                };
                
                // Serialize data
                await using var serializedConfigStream = await serializer.SerializeAsync(configurationDataModel, cancellationToken);

                // Write configuration
                await serializedConfigStream.CopyToAsync(configStream, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _macKey?.Dispose();
        }
    }
}
