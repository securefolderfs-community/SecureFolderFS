using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utilities;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.VaultAccess
{
    // TODO: Needs docs
    public sealed class VaultReader
    {
        private readonly IFolder _vaultFolder;
        private readonly IAsyncSerializer<Stream> _serializer;

        public VaultReader(IFolder vaultFolder, IAsyncSerializer<Stream> serializer)
        {
            _vaultFolder = vaultFolder;
            _serializer = serializer;
        }

        public async Task<VaultKeystoreDataModel> ReadKeystoreAsync(CancellationToken cancellationToken)
        {
            // Get keystore file
            var keystoreFile = await _vaultFolder.GetFileAsync(Constants.Vault.Names.VAULT_KEYSTORE_FILENAME, cancellationToken);
            return await ReadDataAsync<VaultKeystoreDataModel>(keystoreFile, cancellationToken);
        }

        public async Task<VaultConfigurationDataModel> ReadConfigurationAsync(CancellationToken cancellationToken)
        {
            // Get configuration file
            var configFile = await _vaultFolder.GetFileAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, cancellationToken);
            return await ReadDataAsync<VaultConfigurationDataModel>(configFile, cancellationToken);
        }

        public async Task<VaultAuthenticationDataModel?> ReadAuthenticationAsync(CancellationToken cancellationToken)
        {
            // Try get authentication file
            var authFile = await _vaultFolder.TryGetFileAsync(Constants.Vault.Names.VAULT_AUTHENTICATION_FILENAME, cancellationToken);
            return authFile is null ? null : await ReadDataAsync<VaultAuthenticationDataModel?>(authFile, cancellationToken);
        }

        private async Task<TData> ReadDataAsync<TData>(IFile file, CancellationToken cancellationToken)
        {
            // Open a stream to the data file
            await using var dataStream = await file.OpenStreamAsync(FileAccess.Read, cancellationToken);

            var data = await _serializer.DeserializeAsync<Stream, TData?>(dataStream, cancellationToken);
            _ = data ?? throw new SerializationException($"Data could not be deserialized into {typeof(TData).Name}.");

            return data;
        }
    }
}
