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
        public WidgetsDataModel GetWidgetsContextForId(string id)
        {
            var hashedId = ChecksumHelpers.CalculateChecksumForId(id);
            return GetSetting<WidgetsDataModel>(() => new(), hashedId);
        }
    }
}
