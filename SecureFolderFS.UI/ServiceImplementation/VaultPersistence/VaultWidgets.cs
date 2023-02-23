using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.VaultPersistence;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.UI.ServiceImplementation.VaultPersistence
{
    /// <inheritdoc cref="IVaultWidgets"/>
    public sealed class VaultsWidgets : SettingsModel, IVaultWidgets
    {
        /// <inheritdoc/>
        protected override IDatabaseModel<string> SettingsDatabase { get; }

        public VaultsWidgets(IModifiableFolder settingsFolder)
        {
            SettingsDatabase = new BatchDatabaseModel(Constants.LocalSettings.VAULTS_WIDGETS_FOLDERNAME, settingsFolder, StreamSerializer.Instance);
        }

        /// <inheritdoc/>
        public void SetWidgetsData(string id, WidgetDataModel? widgetDataModel)
        {
            var hashedId = ChecksumHelpers.CalculateChecksumForId(id);
            SetSetting(widgetDataModel, hashedId);
        }

        /// <inheritdoc/>
        public WidgetsCollectionDataModel? GetWidgetsData(string id)
        {
            var hashedId = ChecksumHelpers.CalculateChecksumForId(id);
            return GetSetting<WidgetsCollectionDataModel>(null, hashedId);
        }
    }
}
