using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.VaultPersistence;
using SecureFolderFS.Sdk.Storage;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultModel"/>
    public sealed class LocalVaultModel : IVaultModel
    {
        private IVaultConfigurations VaultConfigurations { get; } = Ioc.Default.GetRequiredService<IVaultPersistenceService>().VaultConfigurations;

        /// <inheritdoc/>
        public IFolder Folder { get; }

        /// <inheritdoc/>
        public string? VaultName { get; private set; }

        /// <inheritdoc/>
        public DateTime? LastAccessDate { get; private set; }

        public LocalVaultModel(IFolder folder, string? vaultName = null, DateTime? lastAccessDate = null)
        {
            Folder = folder;
            VaultName = vaultName;
            LastAccessDate = lastAccessDate;
        }

        /// <inheritdoc/>
        public bool Equals(IVaultModel? other)
        {
            if (other is null)
                return false;

            return Folder.Id.Equals(other.Folder.Id);
        }

        /// <inheritdoc/>
        public async Task<bool> SetLastAccessDateAsync(DateTime? value, CancellationToken cancellationToken = default)
        {
            if (VaultConfigurations.SavedVaults is null)
                return false;

            var item = VaultConfigurations.SavedVaults.FirstOrDefault(x => x.Id == Folder.Id);
            if (item is null)
                return false;

            item.LastAccessDate = value;
            var result = await VaultConfigurations.SaveAsync(cancellationToken);

            if (result)
                LastAccessDate = value;

            return result;
        }
    }
}
