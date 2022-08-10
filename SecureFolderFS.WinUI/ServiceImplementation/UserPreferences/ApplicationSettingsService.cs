using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.WinUI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="IApplicationSettingsService"/>
    internal sealed class ApplicationSettingsService : SingleFileSettingsModel, IApplicationSettingsService
    {
        public ApplicationSettingsService(IModifiableFolder? settingsFolder)
        {
            SettingsFolder = settingsFolder;
            SettingsDatabase = new BaseDictionaryDatabaseModel(JsonToStreamSerializer.Instance);
        }

        /// <inheritdoc/>
        protected override string? SettingsStorageName { get; } = Constants.LocalSettings.APPLICATION_SETTINGS_FILENAME;
    }
}
