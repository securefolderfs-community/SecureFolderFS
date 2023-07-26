using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Routines.CredentialsRoutines
{
    /// <inheritdoc cref="ICredentialsRoutine"/>
    internal sealed class CredentialsRoutine : ICredentialsRoutine
    {
        private readonly CipherProvider _cipherProvider;
        private readonly VaultConfigurationDataModel _configDataModel;
        private VaultKeystoreDataModel? _keystoreDataModel;
        private VaultKeystoreDataModel? _newKeystoreDataModel;
        private SecretKey? _macKey;

        public CredentialsRoutine(VaultConfigurationDataModel configDataModel)
        {
            _configDataModel = configDataModel;
            _cipherProvider = CipherProvider.CreateNew();
        }

        /// <inheritdoc/>
        public async Task SetKeystoreAsync(Stream keystoreStream, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken)
        {
            _keystoreDataModel = await serializer.DeserializeAsync<Stream, VaultKeystoreDataModel>(keystoreStream, cancellationToken);
            _ = _keystoreDataModel ?? throw new SerializationException($"Couldn't deserialize to {nameof(VaultKeystoreDataModel)}.");
        }

        /// <inheritdoc/>
        public void SetPassword(IPassword existingPassword, IPassword newPassword)
        {
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);

            using (newPassword)
            using (existingPassword)
            {
                using var secureRandom = RandomNumberGenerator.Create();
                using var encKey = new SecureKey(new byte[Constants.KeyChains.ENCKEY_LENGTH]);
                using var macKey = new SecureKey(new byte[Constants.KeyChains.MACKEY_LENGTH]);

                // Derive KEK
                Span<byte> kek = stackalloc byte[Cryptography.Constants.ARGON2_KEK_LENGTH];
                _cipherProvider.Argon2idCrypt.DeriveKey(existingPassword.GetPassword(), _keystoreDataModel.Salt, kek);

                // Unwrap keys
                _cipherProvider.Rfc3394KeyWrap.UnwrapKey(_keystoreDataModel.WrappedEncKey, kek, encKey.Key);
                _cipherProvider.Rfc3394KeyWrap.UnwrapKey(_keystoreDataModel.WrappedMacKey, kek, macKey.Key);

                // Generate new salt
                var salt = new byte[Constants.KeyChains.SALT_LENGTH];
                secureRandom.GetNonZeroBytes(salt);

                // Derive new KEK
                Span<byte> kek2 = stackalloc byte[Cryptography.Constants.ARGON2_KEK_LENGTH];
                _cipherProvider.Argon2idCrypt.DeriveKey(newPassword.GetPassword(), salt, kek2);

                // Wrap keys
                var wrappedEncKey = _cipherProvider.Rfc3394KeyWrap.WrapKey(encKey, kek2);
                var wrappedMacKey = _cipherProvider.Rfc3394KeyWrap.WrapKey(macKey, kek2);

                _newKeystoreDataModel = new()
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
        public async Task WriteKeystoreAsync(Stream keystoreStream, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken)
        {
            keystoreStream.SetLength(0L);
            await using var serializedKeystoreStream = await serializer.SerializeAsync(_newKeystoreDataModel, cancellationToken);
            await serializedKeystoreStream.CopyToAsync(keystoreStream, cancellationToken);
            keystoreStream.Position = 0L;
        }

        /// <inheritdoc/>
        public async Task WriteConfigurationAsync(Stream configStream, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_macKey);

            using (_macKey)
            {
                using var hmacSha256Crypt = _cipherProvider.HmacSha256Crypt.GetInstance();
                hmacSha256Crypt.InitializeHmac(_macKey);
                hmacSha256Crypt.Update(BitConverter.GetBytes(Constants.VaultVersion.LATEST_VERSION));
                hmacSha256Crypt.Update(BitConverter.GetBytes((uint)_configDataModel.FileNameCipherScheme));
                hmacSha256Crypt.Update(BitConverter.GetBytes((uint)_configDataModel.ContentCipherScheme));

                var payloadMac = new byte[_cipherProvider.HmacSha256Crypt.MacSize];
                hmacSha256Crypt.GetHash(payloadMac);

                var configDataModel = new VaultConfigurationDataModel()
                {
                    ContentCipherScheme = _configDataModel.ContentCipherScheme,
                    FileNameCipherScheme = _configDataModel.FileNameCipherScheme,
                    Version = Constants.VaultVersion.LATEST_VERSION,
                    PayloadMac = payloadMac
                };

                // Serialize data
                await using var serializedConfigStream = await serializer.SerializeAsync(configDataModel, cancellationToken);

                // Write configuration
                configStream.SetLength(0L);
                await serializedConfigStream.CopyToAsync(configStream, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _macKey?.Dispose();
            _cipherProvider.Dispose();   
        }
    }
}
