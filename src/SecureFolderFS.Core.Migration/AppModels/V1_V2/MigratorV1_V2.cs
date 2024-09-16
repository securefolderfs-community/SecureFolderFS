using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Migration.AppModels.V1_V2
{
    /// <inheritdoc cref="IVaultMigratorModel"/>
    internal sealed class MigratorV1_V2 : IVaultMigratorModel
    {
        private readonly IAsyncSerializer<Stream> _streamSerializer;
        private V1VaultConfigurationDataModel? _v1ConfigDataModel;
        private VaultKeystoreDataModel? _v1KeystoreDataModel;

        /// <inheritdoc/>
        public IFolder VaultFolder { get; }

        public MigratorV1_V2(IFolder vaultFolder, IAsyncSerializer<Stream> streamSerializer)
        {
            VaultFolder = vaultFolder;
            _streamSerializer = streamSerializer;
        }

        /// <inheritdoc/>
        public async Task<IDisposable> UnlockAsync<T>(T credentials, CancellationToken cancellationToken = default)
        {
            if (credentials is not IPassword password)
                throw new ArgumentException($"Argument {credentials} is not of type {typeof(IPassword)}.");

            var configFile = await VaultFolder.GetFileByNameAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, cancellationToken);
            var keystoreFile = await VaultFolder.GetFileByNameAsync(Constants.Vault.Names.VAULT_KEYSTORE_FILENAME, cancellationToken);

            await using var configStream = await configFile.OpenReadAsync(cancellationToken);
            await using var keystoreStream = await keystoreFile.OpenReadAsync(cancellationToken);

            _v1ConfigDataModel = await _streamSerializer.DeserializeAsync<Stream, V1VaultConfigurationDataModel>(configStream, cancellationToken);
            _v1KeystoreDataModel = await _streamSerializer.DeserializeAsync<Stream, VaultKeystoreDataModel>(keystoreStream, cancellationToken);
            if (_v1KeystoreDataModel is null)
                throw new FormatException($"{nameof(VaultKeystoreDataModel)} was not in the correct format.");

            
            var kek = new byte[Cryptography.Constants.ARGON2_KEK_LENGTH];
            using var encKey = new SecureKey(Cryptography.Constants.KeyChains.ENCKEY_LENGTH);
            using var macKey = new SecureKey(Cryptography.Constants.KeyChains.MACKEY_LENGTH);

            Argon2id.DeriveKey(password.ToArray(), _v1KeystoreDataModel.Salt, kek);

            // Unwrap keys
            Rfc3394KeyWrap.UnwrapKey(_v1KeystoreDataModel.WrappedEncKey, kek, encKey.Key);
            Rfc3394KeyWrap.UnwrapKey(_v1KeystoreDataModel.WrappedMacKey, kek, macKey.Key);

            // Create copies of keys for later use
            return new EncAndMacKey(encKey.CreateCopy(), macKey.CreateCopy());
        }

        /// <inheritdoc/>
        public async Task MigrateAsync(IDisposable unlockContract, ProgressModel progress, CancellationToken cancellationToken = default)
        {
            if (_v1ConfigDataModel is null)
                throw new InvalidOperationException($"{nameof(_v1ConfigDataModel)} is null.");

            if (unlockContract is not EncAndMacKey encAndMacKey)
                throw new ArgumentException($"{nameof(unlockContract)} is not of correct type.");

            // Begin progress report
            progress.PrecisionProgress?.Report(0d);

            var vaultId = Guid.NewGuid().ToString();
            var v2ConfigDataModel = new VaultConfigurationDataModel()
            {
                AuthenticationMethod = Constants.Vault.Authentication.AUTH_PASSWORD,
                ContentCipherId = GetContentCipherId(_v1ConfigDataModel.ContentCipherScheme),
                FileNameCipherId = GetFileNameCipherId(_v1ConfigDataModel.FileNameCipherScheme),
                PayloadMac = new byte[HMACSHA256.HashSizeInBytes],
                Uid = vaultId,
                Version = Constants.Vault.Versions.V2
            };

            // Calculate and update configuration MAC
            VaultParser.CalculateConfigMac(v2ConfigDataModel, encAndMacKey.MacKey, v2ConfigDataModel.PayloadMac);

            var configFile = await VaultFolder.GetFileByNameAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, cancellationToken);
            await using var configStream = await configFile.OpenReadWriteAsync(cancellationToken);

            // Create backup
            await CreateConfigBackup(configStream, cancellationToken);

            await using var serializedStream = await _streamSerializer.SerializeAsync(v2ConfigDataModel, cancellationToken);
            await serializedStream.CopyToAsync(configStream, cancellationToken);

            // End progress report
            progress.PrecisionProgress?.Report(100d);
        }

        private async Task CreateConfigBackup(Stream configStream, CancellationToken cancellationToken)
        {
            if (VaultFolder is not IModifiableFolder modifiableFolder)
                return;

            var backupConfigName = $"{Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME}.bkup";
            var backupConfigFile = await modifiableFolder.CreateFileAsync(backupConfigName, true, cancellationToken);
            await using var backupConfigStream = await backupConfigFile.OpenWriteAsync(cancellationToken);

            await configStream.CopyToAsync(backupConfigStream, cancellationToken);
            configStream.Position = 0L;
        }

        private static string GetContentCipherId(int v1ContentCipherScheme)
        {
            return v1ContentCipherScheme switch
            {
                1 => Cryptography.Constants.CipherId.AES_CTR_HMAC,
                2 => Cryptography.Constants.CipherId.AES_GCM,
                4 => Cryptography.Constants.CipherId.XCHACHA20_POLY1305,
                _ => throw new ArgumentOutOfRangeException(nameof(v1ContentCipherScheme))
            };
        }

        private static string GetFileNameCipherId(int v1FileNameCipherScheme)
        {
            return v1FileNameCipherScheme switch
            {
                1 => Cryptography.Constants.CipherId.NONE,
                2 => Cryptography.Constants.CipherId.AES_SIV,
                _ => throw new ArgumentOutOfRangeException(nameof(v1FileNameCipherScheme))
            };
        }

        private sealed class EncAndMacKey(SecretKey encKey, SecretKey macKey) : IDisposable
        {
            public SecretKey EncKey { get; } = encKey;

            public SecretKey MacKey { get; } = macKey;

            /// <inheritdoc/>
            public void Dispose()
            {
                EncKey.Dispose();
                MacKey.Dispose();
            }
        }
    }
}
