using OwlCore.Storage;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.VaultAccess
{
    // TODO: Needs docs
    public sealed class VaultWriter
    {
        private readonly IFolder _vaultFolder;
        private readonly IAsyncSerializer<Stream> _serializer;

        public VaultWriter(IFolder vaultFolder, IAsyncSerializer<Stream> serializer)
        {
            _vaultFolder = vaultFolder;
            _serializer = serializer;
        }

        public async Task WriteKeystoreAsync(VaultKeystoreDataModel? keystoreDataModel, CancellationToken cancellationToken)
        {
            var keystoreFile = keystoreDataModel is null ? null : _vaultFolder switch
            {
                IModifiableFolder modifiableFolder => await modifiableFolder.CreateFileAsync(Constants.Vault.Names.VAULT_KEYSTORE_FILENAME, true, cancellationToken),
                _ => await _vaultFolder.GetFirstByNameAsync(Constants.Vault.Names.VAULT_KEYSTORE_FILENAME, cancellationToken) as IFile
            };

            await WriteDataAsync(keystoreFile, keystoreDataModel, cancellationToken);
        }

        public async Task WriteConfigurationAsync(VaultConfigurationDataModel? configDataModel, CancellationToken cancellationToken)
        {
            var configFile = configDataModel is null ? null : _vaultFolder switch
            {
                IModifiableFolder modifiableFolder => await modifiableFolder.CreateFileAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, true, cancellationToken),
                _ => await _vaultFolder.GetFirstByNameAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, cancellationToken) as IFile
            };

            await WriteDataAsync(configFile, configDataModel, cancellationToken);
        }

        public async Task WriteAuthenticationAsync(string fileName, VaultChallengeDataModel? authDataModel, CancellationToken cancellationToken)
        {
            var authFile = authDataModel is null ? null : _vaultFolder switch
            {
                IModifiableFolder modifiableFolder => await modifiableFolder.CreateFileAsync(fileName, true, cancellationToken),
                _ => await _vaultFolder.GetFirstByNameAsync(fileName, cancellationToken) as IFile
            };

            await WriteDataAsync(authFile, authDataModel, cancellationToken);
        }

        private async Task WriteDataAsync<TData>(IFile? file, TData? data, CancellationToken cancellationToken)
        {
            if (file is null)
                return;

            // Open a stream to the data file
            await using var fileStream = await file.OpenStreamAsync(FileAccess.ReadWrite, cancellationToken);

            // Clear contents if opened from existing file
            fileStream.TrySetLength(0L);

            if (data is not null)
            {
                await using var serializedData = await _serializer.SerializeAsync(data, cancellationToken);
                await serializedData.CopyToAsync(fileStream, cancellationToken);
            }
        }
    }
}
