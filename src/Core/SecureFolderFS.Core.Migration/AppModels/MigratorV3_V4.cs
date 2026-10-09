using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Migration.DataModels;
using SecureFolderFS.Core.Migration.Helpers;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Shared.SecureStore;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Core.Migration.AppModels
{
    /// <inheritdoc cref="IVaultMigratorModel"/>
    internal sealed class MigratorV3_V4 : IVaultMigratorModel
    {
        private readonly IAsyncSerializer<Stream> _streamSerializer;
        private V3VaultConfigurationDataModel? _v3ConfigDataModel; // A verified data model

        /// <inheritdoc/>
        public IFolder VaultFolder { get; }

        public MigratorV3_V4(IFolder vaultFolder, IAsyncSerializer<Stream> streamSerializer)
        {
            VaultFolder = vaultFolder;
            _streamSerializer = streamSerializer;
        }

        /// <inheritdoc/>
        public async Task<IDisposable> UnlockAsync(IKeyBytes credentials, CancellationToken cancellationToken = default)
        {
            var configDataModel = await ReadConfigurationAsync(cancellationToken);
            var keystoreDataModel = await ReadKeystoreAsync(cancellationToken);

            byte[] dekKey;
            byte[] macKey;
            var passkey = credentials.UseKey(static key => key.ToArray());
            try
            {
                (dekKey, macKey) = MigrationVaultParser.V3DeriveKeystore(passkey, keystoreDataModel);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(passkey);
            }

            using var dek = SecureKey.TakeOwnership(dekKey);
            using var mac = SecureKey.TakeOwnership(macKey);

            // The migration re-signs the configuration with the vault's real MAC key. Verifying the existing
            // signature beforehand makes sure a tampered V3 configuration cannot be laundered into a valid one
            VerifyConfiguration(configDataModel, mac);

            // Retain the configuration for later use, only on success
            _v3ConfigDataModel = configDataModel;

            // Create copies of keys for later use
            return KeyPair.ImportKeys(dek, mac);
        }

        /// <inheritdoc/>
        public async Task<IDisposable> RecoverAsync(string encodedRecoveryKey, CancellationToken cancellationToken = default)
        {
            using var recoveryKey = KeyPair.CombineRecoveryKey(encodedRecoveryKey);
            using var keyPair = KeyPair.CopyFromRecoveryKey(recoveryKey);

            // The keystore is carried over unchanged, so unlike the V2 to V3 migration, recovering here does not require new credentials to be configured
            var configDataModel = await ReadConfigurationAsync(cancellationToken);
            VerifyConfiguration(configDataModel, keyPair.MacKey);

            // Retain the configuration for later use, only on success
            _v3ConfigDataModel = configDataModel;

            // Create copies of keys and dispose of the original instance
            return keyPair.CreateCopy();
        }

        /// <inheritdoc/>
        public async Task MigrateAsync(IDisposable unlockContract, ProgressModel<IResult> progress, CancellationToken cancellationToken = default)
        {
            _ = _v3ConfigDataModel ?? throw new InvalidOperationException($"{nameof(_v3ConfigDataModel)} is null.");

            if (unlockContract is not KeyPair keyPair)
                throw new ArgumentException($"{nameof(unlockContract)} is not of the correct type.");

            // Begin progress report
            progress.PercentageProgress?.Report(0d);

            // File Names.
            //
            // Names are converted before the configuration is bumped to V4. An interrupted run therefore
            // leaves behind a vault that still declares V3 and can be migrated again, rather than one that
            // declares V4 while part of its content is still encoded the old way. The conversion itself is
            // idempotent, so repeating it only picks up where it left off
            await ConvertFileNamesAsync(progress, cancellationToken);

            // Vault Configuration.
            //
            var v4ConfigDataModel = new VaultConfigurationDataModel()
            {
                ContentCipherId = _v3ConfigDataModel.ContentCipherId,
                FileNameCipherId = _v3ConfigDataModel.FileNameCipherId,
                FileNameEncodingId = _v3ConfigDataModel.FileNameEncodingId,

                // V3 predates file name shortening, so no name in the vault is stored in shortened form.
                // A threshold of zero keeps shortening disabled, matching the existing ciphertext layout
                ShorteningThreshold = 0,
                RecycleBinSize = _v3ConfigDataModel.RecycleBinSize,
                AuthenticationMethod = _v3ConfigDataModel.AuthenticationMethod,
                Uid = _v3ConfigDataModel.Uid,

                // Both App Platform vaults and credential complementation postdate V3
                AppPlatform = null,
                ComplementGeneration = 0,
                Version = Constants.Vault.Versions.V4
            };

            // Re-sign the payload, since V4 covers the shortening threshold that V3 did not have
            var payloadMac = new byte[HMACSHA256.HashSizeInBytes];
            keyPair.MacKey.UseKey(macKey => VaultParser.CalculateConfigMac(v4ConfigDataModel, macKey, payloadMac));
            v4ConfigDataModel.PayloadMac = payloadMac;

            var configFile = await VaultFolder.GetFileByNameAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, cancellationToken);
            await using var configStream = await configFile.OpenReadWriteAsync(cancellationToken);

            // Create backup. The keystore is not modified by this migration and thus needs no backup
            if (VaultFolder is IModifiableFolder modifiableFolder)
            {
                await BackupHelpers.CreateBackup(
                    modifiableFolder,
                    Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME,
                    Constants.Vault.Versions.V3,
                    configStream,
                    cancellationToken);
            }

            // Serialize before truncating so a failure here cannot leave behind an empty configuration
            await using var serializedConfigStream = await _streamSerializer.SerializeAsync(v4ConfigDataModel, cancellationToken);

            // Reset length
            configStream.SetLength(0L);

            // Copy serialized output
            await serializedConfigStream.CopyToAsync(configStream, cancellationToken);

            // End progress report
            progress.PercentageProgress?.Report(100d);
        }

        /// <summary>
        /// Re-encodes the vault's ciphertext names when they were written with the Base4K implementation used before V4.
        /// </summary>
        /// <remarks>
        /// The two Base4K implementations cannot read one another's output, so a Base4K vault whose names were
        /// left alone would mount with every item unreadable. Names encoded as Base64Url, and vaults that do not
        /// encrypt names at all, are unaffected and skipped.
        /// </remarks>
        private async Task ConvertFileNamesAsync(ProgressModel<IResult> progress, CancellationToken cancellationToken)
        {
            _ = _v3ConfigDataModel ?? throw new InvalidOperationException($"{nameof(_v3ConfigDataModel)} is null.");

            // Without name encryption, names are stored in plaintext and carry no encoding
            if (string.IsNullOrEmpty(_v3ConfigDataModel.FileNameCipherId))
                return;

            if (!string.Equals(_v3ConfigDataModel.FileNameEncodingId, Cryptography.Constants.CipherId.ENCODING_BASE4K, StringComparison.Ordinal))
                return;

            var contentFolder = await VaultFolder.TryGetFolderByNameAsync(Constants.Vault.Names.VAULT_CONTENT_FOLDERNAME, cancellationToken);
            if (contentFolder is null)
                return;

            await Base4KNameMigrator.ConvertAsync(contentFolder, progress.PercentageProgress, cancellationToken);
        }

        private async Task<V3VaultConfigurationDataModel> ReadConfigurationAsync(CancellationToken cancellationToken)
        {
            var configFile = await VaultFolder.GetFileByNameAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, cancellationToken);
            await using var configStream = await configFile.OpenReadAsync(cancellationToken);

            var configDataModel = await _streamSerializer.TryDeserializeAsync<Stream, V3VaultConfigurationDataModel>(configStream, cancellationToken);
            if (configDataModel is null)
                throw new FormatException($"{nameof(V3VaultConfigurationDataModel)} was not in the correct format.");

            if (configDataModel.Version != Constants.Vault.Versions.V3)
                throw new FormatException($"Expected a vault of version {Constants.Vault.Versions.V3} but got {configDataModel.Version}.");

            return configDataModel;
        }

        private async Task<V3VaultKeystoreDataModel> ReadKeystoreAsync(CancellationToken cancellationToken)
        {
            var keystoreFile = await VaultFolder.GetFileByNameAsync(Constants.Vault.Names.VAULT_KEYSTORE_FILENAME, cancellationToken);
            await using var keystoreStream = await keystoreFile.OpenReadAsync(cancellationToken);

            var keystoreDataModel = await _streamSerializer.TryDeserializeAsync<Stream, V3VaultKeystoreDataModel>(keystoreStream, cancellationToken);
            if (keystoreDataModel is null)
                throw new FormatException($"{nameof(V3VaultKeystoreDataModel)} was not in the correct format.");

            return keystoreDataModel;
        }

        private static void VerifyConfiguration(V3VaultConfigurationDataModel configDataModel, IKeyUsage macKey)
        {
            var isEqual = macKey.UseKey(key =>
            {
                Span<byte> payloadMac = stackalloc byte[HMACSHA256.HashSizeInBytes];
                MigrationVaultParser.V3CalculateConfigMac(configDataModel, key, payloadMac);

                // Check if stored hash equals to computed hash
                return CryptographicOperations.FixedTimeEquals(payloadMac, configDataModel.PayloadMac ?? []);
            });

            if (!isEqual)
                throw new CryptographicException("Vault hash doesn't match the computed hash.");
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
