using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.UI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="IVaultsSettingsService"/>
    public sealed class VaultsSettingsService : OnDeviceSettingsModel, IVaultsSettingsService
    {
        public VaultsSettingsService(IModifiableFolder settingsFolder)
            : base(settingsFolder)
        {
        }

        /// <inheritdoc/>
        public VaultContextDataModel GetVaultContextForId(string id)
        {
            var vaultContext = GetSetting<VaultContextDataModel>(() => new(), id);
            return vaultContext!; // TODO: Ensure not null
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var folderOfSettings = await SettingsFolder.TryCreateFolderAsync(Constants.LocalSettings.VAULTS_SETTINGS_FOLDERNAME, false, cancellationToken);
            if (folderOfSettings is not IModifiableFolder modifiableFolderOfSettings)
                return;

            SettingsDatabase = new MultipleFilesDatabaseModel(modifiableFolderOfSettings, StreamSerializer.Instance);
            IsAvailable = true;
        }
    }
}
