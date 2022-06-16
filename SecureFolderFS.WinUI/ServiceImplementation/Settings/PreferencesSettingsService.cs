using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.WinUI.Storage;

namespace SecureFolderFS.WinUI.ServiceImplementation.Settings
{
    /// <inheritdoc cref="IPreferencesSettingsService"/>
    internal sealed class PreferencesSettingsService : SettingsModel, IPreferencesSettingsService
    {
        public PreferencesSettingsService(ISettingsServiceInternal settingsServiceInternal)
        {
            _ = settingsServiceInternal; // TODO: Use this parameter
            FilePool = new CachingFilePool(null, null); // TODO: Add parameters
            SettingsDatabase = new DictionarySettingsDatabaseModel(new JsonToStreamSerializer());
        }

        /// <inheritdoc/>
        protected override string SettingsStorageName { get; } = "user_settings2.json";

        /// <inheritdoc/>
        public FileSystemAdapterType ActiveFileSystemAdapter
        {
            get => (FileSystemAdapterType)GetSetting<uint>(() => (uint)FileSystemAdapterType.DokanAdapter);
            set => SetSetting<uint>((uint)value);
        }

        /// <inheritdoc/>
        public bool StartOnSystemStartup
        {
            get => GetSetting(() => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public bool ContinueOnLastVault
        {
            get => GetSetting(() => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public bool AutoOpenVaultFolder 
        { 
            get => GetSetting(() => false); 
            set => SetSetting(value);
        }
    }
}
