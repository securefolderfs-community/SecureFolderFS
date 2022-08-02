using System.Collections.Generic;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.WinUI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="IVaultsSettingsService"/>
    internal sealed class VaultsSettingsService : SingleFileSettingsModel, IVaultsSettingsService
    {
        public VaultsSettingsService(IModifiableFolder? settingsFolder)
        {
            SettingsFolder = settingsFolder;
            SettingsDatabase = new DictionarySettingsDatabaseModel(JsonToStreamSerializer.Instance);
            IsAvailable = settingsFolder is not null;
        }

        /// <inheritdoc/>
        protected override string? SettingsStorageName { get; } = Constants.LocalSettings.VAULTS_SETTINGS_FILENAME;

        /// <inheritdoc/>
        public Dictionary<string, VaultContextDataModel>? VaultContexts
        {
            get => GetSetting<Dictionary<string, VaultContextDataModel>?>(() => null);
            set => SetSetting<Dictionary<string, VaultContextDataModel>?>(value);
        }

        public Dictionary<string, WidgetsContextDataModel>? WidgetContexts
        {
            get => GetSetting<Dictionary<string, WidgetsContextDataModel>?>(() => null);
            set => SetSetting<Dictionary<string, WidgetsContextDataModel>?>(value);
        }
    }
}
