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
        public SettingsService(IFilePool settingsFilePool)
        {
            FilePool = settingsFilePool;
            SettingsDatabase = new DictionarySettingsDatabaseModel(JsonToStreamSerializer.Instance);
            IsAvailable = true;
        }

        /// <inheritdoc/>
        protected override string? SettingsStorageName { get; } = Constants.LocalSettings.USER_SETTINGS_FILE_NAME;

        /// <inheritdoc/>
        public Dictionary<VaultIdModel, VaultViewModel> SavedVaults
        {
            get => GetSetting<List<KeyValuePair<VaultIdModel, VaultViewModel>>>(() => new())!.ToDictionary()!;
            set => SetSetting<List<KeyValuePair<VaultIdModel, VaultViewModel>>>(value.ToList());
        }

        internal ISettingsDatabaseModel GetDatabaseModel()
        {
            return SettingsDatabase!;
        }
    }
}
