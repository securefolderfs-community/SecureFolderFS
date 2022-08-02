using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.WinUI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="ISettingsService"/>
    internal sealed class SettingsService : SingleFileSettingsModel, ISettingsService
    {
        public SettingsService(IModifiableFolder? settingsFolder)
        {
            SettingsFolder = settingsFolder;
            SettingsDatabase = new DictionarySettingsDatabaseModel(JsonToStreamSerializer.Instance);
            IsAvailable = settingsFolder is not null;
        }

        /// <inheritdoc/>
        protected override string? SettingsStorageName { get; } = Constants.LocalSettings.USER_SETTINGS_FILENAME;

        internal ISettingsDatabaseModel GetDatabaseModel()
        {
            return SettingsDatabase!;
        }
    }
}
