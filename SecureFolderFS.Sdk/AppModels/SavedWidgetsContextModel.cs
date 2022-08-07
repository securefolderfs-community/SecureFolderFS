using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IWidgetsContextModel"/>
    public sealed class SavedWidgetsContextModel : IWidgetsContextModel
    {
        private IVaultsSettingsService VaultsSettingsService { get; } = Ioc.Default.GetRequiredService<IVaultsSettingsService>();

        /// <inheritdoc/>
        public IVaultModel VaultModel { get; }

        public SavedWidgetsContextModel(IVaultModel vaultModel)
        {
            VaultModel = vaultModel;
        }

        /// <inheritdoc/>
        public Task<IWidgetModel?> GetOrCreateWidgetAsync(string widgetId, CancellationToken cancellationToken = default)
        {
            if (VaultModel.Folder is not ILocatableFolder vaultFolder)
                return Task.FromResult<IWidgetModel?>(null);

            var widgetsContext = VaultsSettingsService.GetWidgetsContextForId(vaultFolder.Path);
            var widgetDataModel = widgetsContext.WidgetDataModels.GetOrCreate(widgetId, static () => new());

            return Task.FromResult<IWidgetModel?>(new LocalWidgetModel(widgetId, VaultsSettingsService, widgetDataModel));
        }

        /// <inheritdoc/>
        public Task<bool> RemoveWidgetAsync(string widgetId, CancellationToken cancellationToken = default)
        {
            if (VaultModel.Folder is not ILocatableFolder vaultFolder)
                return Task.FromResult(false);

            var widgetsContext = VaultsSettingsService.GetWidgetsContextForId(vaultFolder.Path);
            var removed = widgetsContext.WidgetDataModels.Remove(widgetId);

            return Task.FromResult(removed);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IWidgetModel> GetWidgetsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (VaultModel.Folder is not ILocatableFolder vaultFolder) 
                yield break;

            var widgetsContext = VaultsSettingsService.GetWidgetsContextForId(vaultFolder.Path);
            foreach (var item in widgetsContext.WidgetDataModels)
            {
                yield return new LocalWidgetModel(item.Key, VaultsSettingsService, item.Value);
            }

            await Task.CompletedTask;
        }
    }
}
