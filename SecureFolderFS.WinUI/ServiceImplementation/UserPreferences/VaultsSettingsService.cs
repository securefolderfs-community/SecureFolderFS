using System.Collections.Generic;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.Storage.StoragePool;

namespace SecureFolderFS.WinUI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="IVaultsSettingsService"/>
    internal sealed class VaultsSettingsService : SingleFileSettingsModel, IVaultsSettingsService
    {
        public VaultsSettingsService(IFilePool? settingsFilePool)
        {
            FilePool = settingsFilePool;
            SettingsDatabase = new DictionarySettingsDatabaseModel(JsonToStreamSerializer.Instance);
            IsAvailable = settingsFilePool is not null;
        }

        /// <inheritdoc/>
        protected override string? SettingsStorageName { get; } = Constants.LocalSettings.VAULTS_SETTINGS_FILENAME;

        /// <inheritdoc/>
        public Dictionary<string, VaultContextDataModel>? VaultContexts
        {
            get => GetSetting<Dictionary<string, VaultContextDataModel>?>(() => null);
            set => SetSetting<Dictionary<string, VaultContextDataModel>?>(value);
        }
    }
}
