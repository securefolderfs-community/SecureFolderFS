using SecureFolderFS.Backend.Services.Settings;
using SecureFolderFS.WinUI.Serialization;
using SecureFolderFS.WinUI.Serialization.Implementation;
using System;
using System.IO;
using Windows.Storage;

namespace SecureFolderFS.WinUI.ServiceImplementation.Settings
{
    internal sealed class ApplicationSettingsService : BaseJsonSettings, IApplicationSettingsService
    {
        public ApplicationSettingsService()
        {
            SettingsSerializer = new DefaultSettingsSerializer();
            JsonSettingsSerializer = new DefaultJsonSettingsSerializer();
            JsonSettingsDatabase = new CachingJsonSettingsDatabase(SettingsSerializer, JsonSettingsSerializer);

            Initialize(Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.LocalSettings.SETTINGS_FOLDER_NAME, Constants.LocalSettings.APPLICATION_SETTINGS_FILENAME));
        }

        public DateTime UpdateLastChecked
        {
            get => Get<DateTime>(() => new());
            set => Set<DateTime>(value);
        }
    }
}
