using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.UI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="IVaultWidgetsService"/>
    public sealed class VaultsWidgetsService : LocalSettingsModel, IVaultWidgetsService
    {
        public VaultsWidgetsService(IModifiableFolder settingsFolder)
            : base(settingsFolder)
        {
        }

        /// <inheritdoc/>
        public WidgetsContextDataModel GetWidgetsContextForId(string id)
        {
            var widgetsContext = GetSetting<WidgetsContextDataModel>(() => new(), id);
            return widgetsContext!; // TODO: Ensure not null
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var folderOfSettings = await SettingsFolder.TryCreateFolderAsync(Constants.LocalSettings.VAULTS_WIDGETS_FOLDERNAME, false, cancellationToken);
            if (folderOfSettings is not IModifiableFolder modifiableFolderOfSettings)
                return;

            SettingsDatabase = new BatchDatabaseModel(modifiableFolderOfSettings, StreamSerializer.Instance);
            IsAvailable = true;
        }
    }
}
