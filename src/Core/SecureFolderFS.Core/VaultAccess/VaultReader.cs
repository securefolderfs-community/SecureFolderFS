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

        public async Task<V4VaultConfigurationDataModel> ReadConfigurationAsync(CancellationToken cancellationToken)
        {
            return await ReadConfigurationAsync<V4VaultConfigurationDataModel>(cancellationToken);
        }

        public async Task<V4VaultKeystoreDataModel> ReadKeystoreAsync(CancellationToken cancellationToken)
        {
            return await ReadKeystoreAsync<V4VaultKeystoreDataModel>(cancellationToken);
        }

        /// <summary>
        /// Reads the keystore as the specified type.
        /// </summary>
        public async Task<TKeystore> ReadKeystoreAsync<TKeystore>(CancellationToken cancellationToken)
            where TKeystore : class
        {
            // Get the keystore file
            var keystoreFile = await _vaultFolder.GetFileByNameAsync(Constants.Vault.Names.VAULT_KEYSTORE_FILENAME, cancellationToken);
            return await ReadDataAsync<TKeystore>(keystoreFile, _serializer, cancellationToken);
        }

        /// <summary>
        /// Reads the configuration file as the specified type.
        /// </summary>
        public async Task<TConfiguration> ReadConfigurationAsync<TConfiguration>(CancellationToken cancellationToken)
            where TConfiguration : class
        {
            // Get configuration file
            var configFile = await _vaultFolder.GetFileByNameAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, cancellationToken);
            return await ReadDataAsync<TConfiguration>(configFile, _serializer, cancellationToken);
        }

        public async Task<VaultSharesDataModel?> ReadComplementationAsync(CancellationToken cancellationToken)
        {
            var complementFile = await _vaultFolder.TryGetFileByNameAsync(Constants.Vault.Names.VAULT_COMPLEMENTATION_FILENAME, cancellationToken);
            if (complementFile is null)
                return null;

            return await ReadDataAsync<VaultSharesDataModel?>(complementFile, _serializer, cancellationToken);
        }

        public async Task<VersionDataModel> ReadVersionAsync(CancellationToken cancellationToken)
        {
            // Get configuration file
            var configFile = await _vaultFolder.GetFileByNameAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, cancellationToken);
            return await ReadDataAsync<VersionDataModel>(configFile, _serializer, cancellationToken);
        }

        public async Task<TCapability?> ReadAuthenticationAsync<TCapability>(string fileName, CancellationToken cancellationToken)
            where TCapability : VaultCapabilityDataModel
        {
            try
            {
                // Try to get authentication file
                var authFile = await _vaultFolder.GetFileByNameAsync(fileName, cancellationToken);
                return await ReadDataAsync<TCapability?>(authFile, _serializer, cancellationToken);
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
