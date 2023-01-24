using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.UserPreferences;

namespace SecureFolderFS.UI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="IPreferencesSettingsService"/>
    public sealed class PreferencesSettingsService : SharedSettingsModel, IPreferencesSettingsService
    {
        public PreferencesSettingsService(IDatabaseModel<string> originSettingsDatabase, ISettingsModel originSettingsModel)
            : base(originSettingsDatabase, originSettingsModel)
        {
        }

        /// <inheritdoc/>
        public string? PreferredFileSystemId
        {
            get => GetSetting<string?>(() => null);
            set => SetSetting<string?>(value);
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
        public bool OpenFolderOnUnlock
        {
            get => GetSetting(() => true);
            set => SetSetting(value);
        }
    }
}
