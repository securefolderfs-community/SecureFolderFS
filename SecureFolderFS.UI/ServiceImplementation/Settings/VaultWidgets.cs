using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.UI.ServiceImplementation.Settings
{
    /// <inheritdoc cref="IVaultWidgets"/>
    public sealed class VaultsWidgetsService : SettingsModel, IVaultWidgets
    {
        /// <inheritdoc/>
        protected override IDatabaseModel<string> SettingsDatabase { get; }

        public VaultsWidgetsService(IModifiableFolder settingsFolder)
        {
            SettingsDatabase = new BatchDatabaseModel(Constants.LocalSettings.VAULTS_WIDGETS_FOLDERNAME, settingsFolder, StreamSerializer.Instance);
        }

        /// <inheritdoc/>
        public WidgetsDataModel GetWidgetsContextForId(string id)
        {
            var widgetsContext = GetSetting<WidgetsDataModel>(() => new(), id);
            return widgetsContext;
        }
    }
}
