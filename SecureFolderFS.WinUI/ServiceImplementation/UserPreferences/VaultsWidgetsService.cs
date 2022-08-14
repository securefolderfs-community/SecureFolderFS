using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.WinUI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="IVaultsWidgetsService"/>
    internal sealed class VaultsWidgetsService : MultipleFilesSettingsModel, IVaultsWidgetsService
    {
        public VaultsWidgetsService(IModifiableFolder? settingsFolder)
        {
            SettingsFolder = settingsFolder;
        }

        /// <inheritdoc/>
        protected override string? SettingsStorageName { get; } = Constants.LocalSettings.VAULTS_WIDGETS_FOLDERNAME;

        /// <inheritdoc/>
        public WidgetsContextDataModel GetWidgetsContextForId(string id)
        {
            var widgetsContext = GetSetting<WidgetsContextDataModel>(() => new(), id);
            return widgetsContext!; // TODO: Ensure not null
        }
    }
}
