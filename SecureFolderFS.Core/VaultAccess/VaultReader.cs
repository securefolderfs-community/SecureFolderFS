using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.VaultAccess
{
    /// <inheritdoc cref="IVaultReader"/>
    internal sealed class VaultReader : IVaultReader
    {
        private readonly IFolder _vaultFolder;
        private readonly IAsyncSerializer<Stream> _serializer;

        public VaultReader(IFolder vaultFolder, IAsyncSerializer<Stream> serializer)
        {
            _vaultFolder = vaultFolder;
            _serializer = serializer;
        }

        /// <inheritdoc/>
        public async Task<(VaultKeystoreDataModel, VaultConfigurationDataModel)> ReadAsync(CancellationToken cancellationToken)
        {
            // Get configuration file
            var configFile = await _vaultFolder.GetFileAsync(Constants.VAULT_CONFIGURATION_FILENAME, cancellationToken);
            var configDataModel = await ReadConfigurationAsync(configFile, cancellationToken);

            // Get keystore file
            var keystoreFile = await _vaultFolder.GetFileAsync(Constants.VAULT_KEYSTORE_FILENAME, cancellationToken);
            var keystoreDataModel = await ReadKeystoreAsync(keystoreFile, cancellationToken);

            return (keystoreDataModel, configDataModel);
        }

        private async Task<VaultConfigurationDataModel> ReadConfigurationAsync(IFile configFile, CancellationToken cancellationToken)
        {
            // Open a stream to the configuration file
            await using var configStream = await configFile.OpenStreamAsync(FileAccess.Read, cancellationToken);

            // Check if the presumed version is supported
            var versionValidator = new VersionValidator(_serializer);
            var validationResult = await versionValidator.ValidateAsync(configStream, cancellationToken);
            if (!validationResult.Successful)
                throw validationResult.Exception ?? new InvalidDataException("Configuration file or version are not valid.");

            var configDataModel = await _serializer.DeserializeAsync<Stream, VaultConfigurationDataModel?>(configStream, cancellationToken);
            if (configDataModel is null)
                throw new SerializationException($"Data could not be deserialized into {nameof(VaultConfigurationDataModel)}.");

            return configDataModel;
        }

        private async Task<VaultKeystoreDataModel> ReadKeystoreAsync(IFile keystoreFile, CancellationToken cancellationToken)
        {
            // Open a stream to the keystore file
            await using var keystoreStream = await keystoreFile.OpenStreamAsync(FileAccess.Read, cancellationToken);

            var keystoreDataModel = await _serializer.DeserializeAsync<Stream, VaultKeystoreDataModel?>(keystoreStream, cancellationToken);
            if (keystoreDataModel is null)
                throw new SerializationException($"Data could not be deserialized into {nameof(VaultKeystoreDataModel)}.");

            return keystoreDataModel;
        }
    }
}
