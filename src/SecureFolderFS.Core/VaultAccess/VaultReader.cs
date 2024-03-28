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

            return await ReadDataAsync<VaultKeystoreDataModel>(keystoreFile, cancellationToken);
        }

        public async Task<VaultConfigurationDataModel> ReadConfigurationAsync(CancellationToken cancellationToken)
        {
            // Get configuration file
            if (await _vaultFolder.GetFirstByNameAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, cancellationToken) is not IFile configFile)
                throw new FileNotFoundException("The configuration file was not found.");

            return await ReadDataAsync<VaultConfigurationDataModel>(configFile, cancellationToken);
        }

        public async Task<VaultAuthenticationDataModel?> ReadAuthenticationAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Try to get authentication file
                var authFile = await _vaultFolder.GetFileByNameAsync(Constants.Vault.Names.VAULT_AUTHENTICATION_FILENAME, cancellationToken);
                return await ReadDataAsync<VaultAuthenticationDataModel?>(authFile, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
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
