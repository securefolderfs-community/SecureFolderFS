using OwlCore.Storage;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Extensions;
using System;
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
            if (await _vaultFolder.GetFirstByNameAsync(Constants.Vault.Names.VAULT_KEYSTORE_FILENAME, cancellationToken) is not IFile keystoreFile)
                throw new FileNotFoundException("The keystore file was not found.");

            return await ReadDataAsync<VaultKeystoreDataModel>(keystoreFile, _serializer, cancellationToken);
        }

        public async Task<VaultConfigurationDataModel> ReadConfigurationAsync(CancellationToken cancellationToken)
        {
            // Get configuration file
            if (await _vaultFolder.GetFirstByNameAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, cancellationToken) is not IFile configFile)
                throw new FileNotFoundException("The configuration file was not found.");

            return await ReadDataAsync<VaultConfigurationDataModel>(configFile, _serializer, cancellationToken);
        }

        public async Task<VersionDataModel> ReadVersionAsync(CancellationToken cancellationToken)
        {
            // Get configuration file
            if (await _vaultFolder.GetFirstByNameAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, cancellationToken) is not IFile configFile)
                throw new FileNotFoundException("The configuration file was not found.");

            return await ReadDataAsync<VersionDataModel>(configFile, _serializer, cancellationToken);
        }

        public async Task<VaultPasskeyDataModel?> ReadAuthenticationAsync(string fileName, CancellationToken cancellationToken)
        {
            try
            {
                // Try to get authentication file
                var authFile = await _vaultFolder.GetFileByNameAsync(fileName, cancellationToken);
                return await ReadDataAsync<VaultPasskeyDataModel?>(authFile, _serializer, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<TData> ReadDataAsync<TData>(IFile file, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken)
        {
            // Open a stream to the data file
            await using var dataStream = await file.OpenStreamAsync(FileAccess.Read, cancellationToken);

            var data = await serializer.DeserializeAsync<Stream, TData?>(dataStream, cancellationToken);
            _ = data ?? throw new SerializationException($"Data could not be deserialized into {typeof(TData).Name}.");

            return data;
        }
    }
}
