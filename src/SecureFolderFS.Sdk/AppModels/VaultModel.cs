﻿using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.VaultPersistence;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultModel"/>
    [Inject<IVaultPersistenceService>]
    public sealed partial class VaultModel : IVaultModel
    {
        private IVaultConfigurations VaultConfigurations => VaultPersistenceService.VaultConfigurations;

        /// <inheritdoc/>
        public IFolder Folder { get; }

        /// <inheritdoc/>
        public string VaultName { get; private set; }

        /// <inheritdoc/>
        public DateTime? LastAccessDate { get; private set; }

        public VaultModel(IFolder folder, string? vaultName = null, DateTime? lastAccessDate = null)
        {
            ServiceProvider = Ioc.Default;
            Folder = folder;
            VaultName = vaultName ?? folder.Name;
            LastAccessDate = lastAccessDate;
        }

        /// <inheritdoc/>
        public async Task<bool> SetLastAccessDateAsync(DateTime? value, CancellationToken cancellationToken = default)
        {
            var result = await UpdateConfigurationAsync(x => x.LastAccessDate = value, cancellationToken);
            if (result)
                LastAccessDate = value;

            return result;
        }

        /// <inheritdoc/>
        public async Task<bool> SetVaultNameAsync(string value, CancellationToken cancellationToken = default)
        {
            var result = await UpdateConfigurationAsync(x => x.VaultName = value, cancellationToken);
            if (result)
                VaultName = value;

            return result;
        }

        private async Task<bool> UpdateConfigurationAsync(Action<VaultDataModel> updateAction, CancellationToken cancellationToken)
        {
            if (VaultConfigurations.SavedVaults is null)
                return false;

            var item = VaultConfigurations.SavedVaults.FirstOrDefault(x => x.Id == Folder.Id);
            if (item is null)
                return false;

            updateAction.Invoke(item);
            return await VaultConfigurations.TrySaveAsync(cancellationToken);
        }
    }
}
