using System.Collections.Generic;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Sdk.Storage.StoragePool;

namespace SecureFolderFS.WinUI.ServiceImplementation.Settings
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
        public List<VaultModel>? SavedVaults
        {
            get => GetSetting(() => new List<VaultModel>());
            set => SetSetting(value);
        }
    }
}
