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
    internal sealed class MigratorV2_V3 : IVaultMigratorModel, IProgress<IKeyBytes>
    {
        private readonly IAsyncSerializer<Stream> _streamSerializer;
        private V2VaultConfigurationDataModel? _v2ConfigDataModel;
        private VaultKeystoreDataModel? _v2KeystoreDataModel;
        private ManagedKey? _secretKeySequence;
        private bool _wasNewPasswordSet;

        /// <inheritdoc/>
        public IFolder VaultFolder { get; }

        public MigratorV2_V3(IFolder vaultFolder, IAsyncSerializer<Stream> streamSerializer)
        {
            VaultFolder = vaultFolder;
            _streamSerializer = streamSerializer;
        }

        /// <inheritdoc/>
        public void Report(IKeyBytes key)
        {
            _wasNewPasswordSet = true;

            var copy = new byte[key.Length];
            key.Key.AsSpan().CopyTo(copy);
            _secretKeySequence = ManagedKey.TakeOwnership(copy);
        }

        /// <inheritdoc/>
        public async Task<IDisposable> UnlockAsync(IKeyBytes credentials, CancellationToken cancellationToken = default)
        {
            if (credentials is not KeySequence keySequence)
                throw new ArgumentException($"Argument {credentials} is not of type {typeof(KeySequence)}.");

            _secretKeySequence?.Dispose();
            _secretKeySequence = null;

            var secretKeySequence = ManagedKey.TakeOwnership(keySequence.Key);
            var configFile = await VaultFolder.GetFileByNameAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, cancellationToken);
            var keystoreFile = await VaultFolder.GetFileByNameAsync(Constants.Vault.Names.VAULT_KEYSTORE_FILENAME, cancellationToken);

            await using var configStream = await configFile.OpenReadAsync(cancellationToken);
            await using var keystoreStream = await keystoreFile.OpenReadAsync(cancellationToken);

            _v2ConfigDataModel = await _streamSerializer.DeserializeAsync<Stream, V2VaultConfigurationDataModel>(configStream, cancellationToken);
            _v2KeystoreDataModel = await _streamSerializer.DeserializeAsync<Stream, VaultKeystoreDataModel>(keystoreStream, cancellationToken);
            if (_v2KeystoreDataModel is null)
                throw new FormatException($"{nameof(VaultKeystoreDataModel)} was not in the correct format.");

            var kek = new byte[Cryptography.Constants.KeyTraits.ARGON2_KEK_LENGTH];
            using var dekKey = new ManagedKey(Cryptography.Constants.KeyTraits.DEK_KEY_LENGTH);
            using var macKey = new ManagedKey(Cryptography.Constants.KeyTraits.MAC_KEY_LENGTH);

            Argon2id.Old_DeriveKey(secretKeySequence, _v2KeystoreDataModel.Salt, kek);

            // Unwrap keys
            using var rfc3394 = new Rfc3394KeyWrap();
            rfc3394.UnwrapKey(_v2KeystoreDataModel.WrappedDekKey, kek, dekKey.Key);
            rfc3394.UnwrapKey(_v2KeystoreDataModel.WrappedMacKey, kek, macKey.Key);

            // Set the secret key sequence for later use, only on success
            _secretKeySequence = secretKeySequence;

            // Create copies of keys for later use
            return KeyPair.ImportKeys(dekKey, macKey);
        }

        /// <inheritdoc/>
        public async Task<IDisposable> RecoverAsync(string encodedRecoveryKey, CancellationToken cancellationToken = default)
        {
            using var recoveryKey = KeyPair.CombineRecoveryKey(encodedRecoveryKey);
            using var keyPair = KeyPair.CopyFromRecoveryKey(recoveryKey);

            var configFile = await VaultFolder.GetFileByNameAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, cancellationToken);
            var keystoreFile = await VaultFolder.GetFileByNameAsync(Constants.Vault.Names.VAULT_KEYSTORE_FILENAME, cancellationToken);

            await using var configStream = await configFile.OpenReadAsync(cancellationToken);
            await using var keystoreStream = await keystoreFile.OpenReadAsync(cancellationToken);

            _v2ConfigDataModel = await _streamSerializer.DeserializeAsync<Stream, V2VaultConfigurationDataModel>(configStream, cancellationToken);
            _v2KeystoreDataModel = await _streamSerializer.DeserializeAsync<Stream, VaultKeystoreDataModel>(keystoreStream, cancellationToken);
            if (_v2ConfigDataModel is null)
                throw new FormatException($"{nameof(V2VaultConfigurationDataModel)} was not in the correct format.");

            // Initialize HMAC
            using var hmacSha256 = new HMACSHA256(keyPair.MacKey.Key);
            hmacSha256.AppendData(BitConverter.GetBytes(_v2ConfigDataModel.Version));
            hmacSha256.AppendData(BitConverter.GetBytes(CryptHelpers.ContentCipherId(_v2ConfigDataModel.ContentCipherId)));
            hmacSha256.AppendData(BitConverter.GetBytes(CryptHelpers.FileNameCipherId(_v2ConfigDataModel.FileNameCipherId)));
            hmacSha256.AppendData(Encoding.UTF8.GetBytes(_v2ConfigDataModel.Uid));
            hmacSha256.AppendFinalData(Encoding.UTF8.GetBytes(_v2ConfigDataModel.AuthenticationMethod));

            var payloadMac = new byte[HMACSHA256.HashSizeInBytes];
            hmacSha256.GetCurrentHash(payloadMac);

            // Check if stored hash equals to computed hash
            if (!CryptographicOperations.FixedTimeEquals(payloadMac, _v2ConfigDataModel.PayloadMac ?? []))
                throw new CryptographicException("Vault hash doesn't match the computed hash.");

            return KeyPair.ImportKeys(keyPair.DekKey, keyPair.MacKey);
        }

        /// <inheritdoc/>
        public async Task MigrateAsync(IDisposable unlockContract, ProgressModel<IResult> progress, CancellationToken cancellationToken = default)
        {
            _ = _v2ConfigDataModel ?? throw new InvalidOperationException($"{nameof(_v2ConfigDataModel)} is null.");
            _ = _v2KeystoreDataModel ?? throw new InvalidOperationException($"{nameof(_v2KeystoreDataModel)} is null.");
            _ = _secretKeySequence ?? throw new InvalidOperationException($"{nameof(_secretKeySequence)} is null.");

            if (unlockContract is not KeyPair keyPair)
                throw new ArgumentException($"{nameof(unlockContract)} is not of the correct type.");

            // Begin progress report
            progress.PercentageProgress?.Report(0d);

            // Vault Configuration ------------------------------------
            //
            var dekKey = keyPair.DekKey;
            var macKey = keyPair.MacKey;
            var v3ConfigDataModel = new VaultConfigurationDataModel()
            {
                AuthenticationMethod = _wasNewPasswordSet ? Core.Constants.Vault.Authentication.AUTH_PASSWORD : _v2ConfigDataModel.AuthenticationMethod,
                ContentCipherId = _v2ConfigDataModel.ContentCipherId,
                FileNameCipherId = _v2ConfigDataModel.FileNameCipherId,
                FileNameEncodingId = Core.Cryptography.Constants.CipherId.ENCODING_BASE64URL,
                RecycleBinSize = 0L,
                PayloadMac = new byte[HMACSHA256.HashSizeInBytes],
                Uid = _v2ConfigDataModel.Uid,
                Version = Constants.Vault.Versions.V3
            };

            // Initialize HMAC
            using var hmacSha256 = new HMACSHA256(macKey.Key);

            // Update HMAC
            hmacSha256.AppendData(BitConverter.GetBytes(v3ConfigDataModel.Version));
            hmacSha256.AppendData(BitConverter.GetBytes(CryptHelpers.ContentCipherId(v3ConfigDataModel.ContentCipherId)));
            hmacSha256.AppendData(BitConverter.GetBytes(CryptHelpers.FileNameCipherId(v3ConfigDataModel.FileNameCipherId)));
            hmacSha256.AppendData(BitConverter.GetBytes(v3ConfigDataModel.RecycleBinSize));
            hmacSha256.AppendData(Encoding.UTF8.GetBytes(v3ConfigDataModel.FileNameEncodingId));
            hmacSha256.AppendData(Encoding.UTF8.GetBytes(v3ConfigDataModel.Uid));
            hmacSha256.AppendFinalData(Encoding.UTF8.GetBytes(v3ConfigDataModel.AuthenticationMethod));

            // Fill the hash to payload
            hmacSha256.GetCurrentHash(v3ConfigDataModel.PayloadMac);

            // Vault Keystore ------------------------------------
            //
            var kek = new byte[Cryptography.Constants.KeyTraits.ARGON2_KEK_LENGTH];
            Argon2id.V3_DeriveKey(_secretKeySequence, _v2KeystoreDataModel.Salt, kek);

            using var rfc3394 = new Rfc3394KeyWrap();
            var newWrappedDekKek = rfc3394.WrapKey(dekKey, kek);
            var newWrappedMacKek = rfc3394.WrapKey(macKey, kek);

            var v3KeystoreDataModel = new VaultKeystoreDataModel()
            {
                Salt = _v2KeystoreDataModel.Salt,
                WrappedDekKey = newWrappedDekKek,
                WrappedMacKey = newWrappedMacKek
            };

            var configFile = await VaultFolder.GetFileByNameAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, cancellationToken);
            var keystoreFile = await VaultFolder.GetFileByNameAsync(Constants.Vault.Names.VAULT_KEYSTORE_FILENAME, cancellationToken);
            await using var configStream = await configFile.OpenReadWriteAsync(cancellationToken);
            await using var keystoreStream = await keystoreFile.OpenReadWriteAsync(cancellationToken);

            // Create backup
            if (VaultFolder is IModifiableFolder modifiableFolder)
            {
                await BackupHelpers.CreateBackup(
                    modifiableFolder,
                    Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME,
                    Constants.Vault.Versions.V2,
                    configStream,
                    cancellationToken);

                await BackupHelpers.CreateBackup(
                    modifiableFolder,
                    Constants.Vault.Names.VAULT_KEYSTORE_FILENAME,
                    Constants.Vault.Versions.V2,
                    keystoreStream,
                    cancellationToken);
            }

            await using var serializedConfigStream = await _streamSerializer.SerializeAsync(v3ConfigDataModel, cancellationToken);
            await using var serializedKeystoreStream = await _streamSerializer.SerializeAsync(v3KeystoreDataModel, cancellationToken);

            // Reset length
            configStream.SetLength(0L);
            keystoreStream.SetLength(0L);

            // Copy serialized output
            await serializedConfigStream.CopyToAsync(configStream, cancellationToken);
            await serializedKeystoreStream.CopyToAsync(keystoreStream, cancellationToken);

            // End progress report
            progress.PercentageProgress?.Report(100d);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _secretKeySequence?.Dispose();
        }
    }
}
