using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IWidgetsContextModel"/>
    public sealed class SavedWidgetsContextModel : BaseSerializedDataModel<IVaultsSettingsService>, IWidgetsContextModel
    {
        /// <inheritdoc/>
        public IVaultModel VaultModel { get; }

        public SavedWidgetsContextModel(IVaultModel vaultModel)
        {
            VaultModel = vaultModel;
        }

        /// <inheritdoc/>
        public async Task<bool> AddWidgetAsync(string widgetId, CancellationToken cancellationToken = default)
        {
            if (!await EnsureSettingsLoaded(cancellationToken))
                return false;

            var widgetsContext = SettingsService.GetWidgetsContextForId(VaultModel.Folder.Id);
            widgetsContext.WidgetDataModels.AddOrReplace(widgetId, new());

            return await SettingsService.SaveSettingsAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveWidgetAsync(string widgetId, CancellationToken cancellationToken = default)
        {
            if (!await EnsureSettingsLoaded(cancellationToken))
                return false;

            var widgetsContext = SettingsService.GetWidgetsContextForId(VaultModel.Folder.Id);
            widgetsContext.WidgetDataModels.Remove(widgetId);

            return await SettingsService.SaveSettingsAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IWidgetModel> GetWidgetsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!await EnsureSettingsLoaded(cancellationToken))
                yield break;

            if (VaultModel.Folder is not ILocatableFolder vaultFolder) 
                yield break;

            var widgetsContext = SettingsService.GetWidgetsContextForId(vaultFolder.Path);
            foreach (var item in widgetsContext.WidgetDataModels)
            {
                yield return new LocalWidgetModel(item.Key, SettingsService, item.Value);
            }

            await Task.CompletedTask;
        }
    }
}
