using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.Helpers;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Migration.DataModels;
using SecureFolderFS.Core.Migration.Helpers;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Migration.AppModels
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

            var kek = new byte[Cryptography.Constants.KeyTraits.ARGON2_KEK_LENGTH];
            using var encKey = new SecureKey(Cryptography.Constants.KeyTraits.ENCKEY_LENGTH);
            using var macKey = new SecureKey(Cryptography.Constants.KeyTraits.MACKEY_LENGTH);

            Argon2id.Old_DeriveKey(password.ToArray(), _v1KeystoreDataModel.Salt, kek);

            // Unwrap keys
            using var rfc3394 = new Rfc3394KeyWrap();
            rfc3394.UnwrapKey(_v1KeystoreDataModel.WrappedEncKey, kek, encKey.Key);
            rfc3394.UnwrapKey(_v1KeystoreDataModel.WrappedMacKey, kek, macKey.Key);

            // Create copies of keys for later use
            return new EncAndMacKey(encKey.CreateCopy(), macKey.CreateCopy());
        }

        /// <inheritdoc/>
        public async Task MigrateAsync(IDisposable unlockContract, ProgressModel<IResult> progress, CancellationToken cancellationToken = default)
        {
            if (_v1ConfigDataModel is null)
                throw new InvalidOperationException($"{nameof(_v1ConfigDataModel)} is null.");

            if (unlockContract is not EncAndMacKey encAndMacKey)
                throw new ArgumentException($"{nameof(unlockContract)} is not of correct type.");

            // Begin progress report
            progress.PercentageProgress?.Report(0d);

            // Vault Configuration ------------------------------------
            //
            var macKey = encAndMacKey.MacKey;
            var vaultId = Guid.NewGuid().ToString();
            var v2ConfigDataModel = new V2VaultConfigurationDataModel()
            {
                AuthenticationMethod = Constants.Vault.Authentication.AUTH_PASSWORD,
                ContentCipherId = GetContentCipherId(_v1ConfigDataModel.ContentCipherScheme),
                FileNameCipherId = GetFileNameCipherId(_v1ConfigDataModel.FileNameCipherScheme),
                PayloadMac = new byte[HMACSHA256.HashSizeInBytes],
                Uid = vaultId,
                Version = Constants.Vault.Versions.V2
            };

            // Initialize HMAC
            using var hmacSha256 = new HMACSHA256(macKey.Key);

            // Update HMAC
            hmacSha256.AppendData(BitConverter.GetBytes(Constants.Vault.Versions.LATEST_VERSION));
            hmacSha256.AppendData(BitConverter.GetBytes(CryptHelpers.ContentCipherId(v2ConfigDataModel.ContentCipherId)));
            hmacSha256.AppendData(BitConverter.GetBytes(CryptHelpers.FileNameCipherId(v2ConfigDataModel.FileNameCipherId)));
            hmacSha256.AppendData(Encoding.UTF8.GetBytes(v2ConfigDataModel.Uid));
            hmacSha256.AppendFinalData(Encoding.UTF8.GetBytes(v2ConfigDataModel.AuthenticationMethod));

            // Fill the hash to payload
            hmacSha256.GetCurrentHash(v2ConfigDataModel.PayloadMac);

            var configFile = await VaultFolder.GetFileByNameAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, cancellationToken);
            await using var configStream = await configFile.OpenReadWriteAsync(cancellationToken);

            // Create backup
            if (VaultFolder is IModifiableFolder modifiableFolder)
            {
                await BackupHelpers.CreateBackup(
                    modifiableFolder,
                    Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME,
                    Constants.Vault.Versions.V1,
                    configStream,
                    cancellationToken);

                await BackupHelpers.CreateBackup(
                    modifiableFolder,
                    Constants.Vault.Names.VAULT_KEYSTORE_FILENAME,
                    Constants.Vault.Versions.V1,
                    cancellationToken);
            }

            await using var serializedStream = await _streamSerializer.SerializeAsync(v2ConfigDataModel, cancellationToken);
            await serializedStream.CopyToAsync(configStream, cancellationToken);

            // End progress report
            progress.PercentageProgress?.Report(100d);
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

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
