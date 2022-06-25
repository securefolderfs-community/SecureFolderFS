using System.Collections.Generic;
using System.Linq;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Sdk.Storage.StoragePool;

namespace SecureFolderFS.WinUI.ServiceImplementation.Settings
{
    /// <inheritdoc cref="ISettingsService"/>
    internal sealed class SettingsService : SingleFileSettingsModel, ISettingsService
    {
        public SettingsService(IFilePool? settingsFilePool)
        {
            FilePool = settingsFilePool;
            SettingsDatabase = new DictionarySettingsDatabaseModel(JsonToStreamSerializer.Instance);
            IsAvailable = settingsFilePool is not null;
        }

        /// <inheritdoc/>
        protected override string? SettingsStorageName { get; } = Constants.LocalSettings.USER_SETTINGS_FILENAME;

        /// <inheritdoc/>
        public Dictionary<VaultIdModel, VaultViewModelDeprecated> SavedVaults
        {
            get => GetSetting<List<KeyValuePair<VaultIdModel, VaultViewModelDeprecated>>>(() => new())!.ToDictionary()!;
            set => SetSetting<List<KeyValuePair<VaultIdModel, VaultViewModelDeprecated>>>(value.ToList());
        }

        internal ISettingsDatabaseModel GetDatabaseModel()
        {
            return SettingsDatabase!;
        }
    }
}
