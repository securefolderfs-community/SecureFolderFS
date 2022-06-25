using System.Collections.Generic;
using System.Linq;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Sdk.Storage.StoragePool;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.WinUI.AppModels;

namespace SecureFolderFS.WinUI.ServiceImplementation.Settings
{
    /// <inheritdoc cref="ISecretSettingsService"/>
    internal sealed class SecretSettingsService : SingleFileSettingsModel, ISecretSettingsService
    {
        public SecretSettingsService(IFilePool settingsFilePool)
        {
            FilePool = settingsFilePool;
            SettingsDatabase = new DictionarySettingsDatabaseModel(new DPAPIJsonToStreamSerializer());
            IsAvailable = true;
        }

        /// <inheritdoc/>
        protected override string? SettingsStorageName { get; } = Constants.LocalSettings.SECRET_SETTINGS_FILE_NAME;

        /// <inheritdoc/>
        public Dictionary<VaultIdModel, VaultModelDeprecated> SavedVaultModels
        {
            get => GetSetting<List<KeyValuePair<VaultIdModel, VaultModelDeprecated>>>(() => new())!.ToDictionary()!;
            set => SetSetting<List<KeyValuePair<VaultIdModel, VaultModelDeprecated>>>(value.ToList());
        }
    }
}
