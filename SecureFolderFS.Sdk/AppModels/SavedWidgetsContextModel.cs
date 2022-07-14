using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.UserPreferences;

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
        public async Task<IWidgetModel?> GetWidgetAsync(string widgetId, CancellationToken cancellationToken = default)
        {
            VaultsSettingsService.WidgetContexts ??= new();
            VaultsSettingsService.WidgetContexts[VaultModel.Folder.Path].WidgetDataModels ??= new();

            if (!VaultsSettingsService.WidgetContexts[VaultModel.Folder.Path].WidgetDataModels!.TryGetValue(widgetId, out var widgetDataModel))
                return null;

            return null;// TODO: widgetDataModel;
        }

        /// <inheritdoc/>
        public Task<bool> RemoveWidgetAsync(string widgetId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
