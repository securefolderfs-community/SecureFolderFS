using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.VaultAccess
{
    /// <inheritdoc cref="IVaultWriter"/>
    internal sealed class VaultWriter : IVaultWriter
    {
        private readonly IFolder _vaultFolder;
        private readonly IAsyncSerializer<Stream> _serializer;

        public VaultWriter(IFolder vaultFolder, IAsyncSerializer<Stream> serializer)
        {
            _vaultFolder = vaultFolder;
            _serializer = serializer;
        }

        /// <inheritdoc/>
        public async Task WriteAsync(VaultKeystoreDataModel? keystoreDataModel, VaultConfigurationDataModel? configDataModel, VaultAuthenticationDataModel? authDataModel, CancellationToken cancellationToken)
        {
            IFile? configFile;
            IFile? keystoreFile;
            IFile? authFile;

            if (_vaultFolder is IModifiableFolder modifiableFolder)
            {
                // Create new or overwrite existing
                configFile = configDataModel is null ? null : await modifiableFolder.CreateFileAsync(Constants.Vault.VAULT_CONFIGURATION_FILENAME, true, cancellationToken);
                keystoreFile = keystoreDataModel is null ? null : await modifiableFolder.CreateFileAsync(Constants.Vault.VAULT_KEYSTORE_FILENAME, true, cancellationToken);
                authFile = authDataModel is null ? null : await modifiableFolder.CreateFileAsync(Constants.Vault.VAULT_AUTHENTICATION_FILENAME, true, cancellationToken);
            }
            else
            {
                // Get existing or fail
                configFile = configDataModel is null ? null : await _vaultFolder.GetFileAsync(Constants.Vault.VAULT_CONFIGURATION_FILENAME, cancellationToken);
                keystoreFile = keystoreDataModel is null ? null : await _vaultFolder.GetFileAsync(Constants.Vault.VAULT_KEYSTORE_FILENAME, cancellationToken);
                authFile = authDataModel is null ? null : await _vaultFolder.GetFileAsync(Constants.Vault.VAULT_AUTHENTICATION_FILENAME, cancellationToken);
            }

            await WriteDataAsync(keystoreFile, keystoreDataModel, cancellationToken);
            await WriteDataAsync(configFile, configDataModel, cancellationToken);
            await WriteDataAsync(authFile, authDataModel, cancellationToken);
        }

        private async Task WriteDataAsync<TData>(IFile? file, TData? data, CancellationToken cancellationToken)
        {
            if (file is null)
                return;

            // Open a stream to the data file
            await using var dataStream = await file.OpenStreamAsync(FileAccess.ReadWrite, cancellationToken);

            // Clear contents if opened from existing file
            dataStream.SetLength(0L);

            await using var serializedData = await _serializer.SerializeAsync(data, cancellationToken);
            await serializedData.CopyToAsync(dataStream, cancellationToken);
        }
    }
}
