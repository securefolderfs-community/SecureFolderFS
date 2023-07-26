using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.DataModels;
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
        public async Task<(VaultKeystoreDataModel, VaultConfigurationDataModel, VaultAuthenticationDataModel?)> ReadAsync(CancellationToken cancellationToken)
        {
            // Get configuration file
            var configFile = await _vaultFolder.GetFileAsync(Constants.VAULT_CONFIGURATION_FILENAME, cancellationToken);
            var configDataModel = await ReadDataAsync<VaultConfigurationDataModel>(configFile, cancellationToken);

            // Get keystore file
            var keystoreFile = await _vaultFolder.GetFileAsync(Constants.VAULT_KEYSTORE_FILENAME, cancellationToken);
            var keystoreDataModel = await ReadDataAsync<VaultKeystoreDataModel>(keystoreFile, cancellationToken);

            // Try get authentication file
            var authFile = await _vaultFolder.TryGetFileAsync(Constants.VAULT_AUTHENTICATION_FILENAME, cancellationToken);
            var authDataModel = authFile is null ? null : await ReadDataAsync<VaultAuthenticationDataModel>(authFile, cancellationToken);

            return (keystoreDataModel, configDataModel, authDataModel);
        }

        private async Task<TData> ReadDataAsync<TData>(IFile file, CancellationToken cancellationToken)
        {
            // Open a stream to the data file
            await using var dataStream = await file.OpenStreamAsync(FileAccess.Read, cancellationToken);

            var data = await _serializer.DeserializeAsync<Stream, TData?>(dataStream, cancellationToken);
            if (data is null)
                throw new SerializationException($"Data could not be deserialized into {typeof(TData).Name}.");

            return data;
        }
    }
}
