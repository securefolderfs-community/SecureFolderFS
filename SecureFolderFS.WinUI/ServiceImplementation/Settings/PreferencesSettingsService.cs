using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.WinUI.Serialization;

namespace SecureFolderFS.WinUI.ServiceImplementation.Settings
{
    /// <inheritdoc cref="IPreferencesSettingsService"/>
    internal sealed class PreferencesSettingsService : SharedSettingsModel, IPreferencesSettingsService
    {
        public PreferencesSettingsService(ISettingsDatabaseModel originSettingsDatabase, ISettingsModel originSettingsModel)
            : base(originSettingsModel)
        {
            SettingsDatabase = originSettingsDatabase;
        }

        /// <inheritdoc/>
        public FileSystemAdapterType ActiveFileSystemAdapter
        {
            get => (FileSystemAdapterType)GetSetting(() => (uint)FileSystemAdapterType.DokanAdapter);
            set => SetSetting((uint)value);
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
