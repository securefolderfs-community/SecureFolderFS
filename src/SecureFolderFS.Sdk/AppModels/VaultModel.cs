using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultModel"/>
    [Inject<IVaultPersistenceService>]
    public sealed partial class VaultModel : IVaultModel
    {
        private readonly IRemoteResource<IFolder>? _remoteVault;

        /// <inheritdoc/>
        public bool IsRemote { get; }

        /// <inheritdoc/>
        public IFolder? VaultFolder { get; private set; }

        /// <inheritdoc/>
        public VaultDataModel DataModel { get; }

        public VaultModel(IFolder folder, VaultDataModel dataModel)
        {
            ServiceProvider = DI.Default;
            VaultFolder = folder;
            DataModel = dataModel;
            IsRemote = false;
        }

        public VaultModel(IRemoteResource<IFolder> remoteVault, VaultDataModel dataModel, IFolder? folder = null)
        {
            ServiceProvider = DI.Default;
            IsRemote = true;
            VaultFolder = folder;
            _remoteVault = remoteVault;
            DataModel = dataModel;
        }

        /// <inheritdoc/>
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await VaultPersistenceService.VaultConfigurations.SaveAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IFolder> ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (VaultFolder is not null)
                return VaultFolder;

            ArgumentNullException.ThrowIfNull(_remoteVault);
            ArgumentNullException.ThrowIfNull(DataModel.PersistableId);

            var rootFolder = await _remoteVault.ConnectAsync(cancellationToken);
            VaultFolder = await rootFolder.GetItemByRelativePathOrSelfAsync(DataModel.PersistableId, cancellationToken) as IFolder;
            if (VaultFolder is null)
                throw new InvalidOperationException("Could not find the vault folder.");

            return VaultFolder;
        }

        /// <inheritdoc/>
        public bool Equals(IVaultModel? other)
        {
            return Equals(other?.DataModel);
        }

        /// <inheritdoc/>
        public bool Equals(VaultDataModel? other)
        {
            return (other?.PersistableId?.Equals(DataModel.PersistableId) ?? false)
                   && (other.StorageSource?.StorageType?.Equals(DataModel.StorageSource?.StorageType) ?? false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (IsRemote)
                VaultFolder = null;

            _remoteVault?.Dispose();
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (IsRemote)
                VaultFolder = null;

            if (_remoteVault is not null)
                await _remoteVault.DisposeAsync();
        }
    }
}
