using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.UI.ServiceImplementation.SettingsPersistence;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="ISettingsService"/>
    public sealed class SettingsService : ISettingsService
    {
        /// <inheritdoc/>
        public IAppSettings AppSettings { get; }

        /// <inheritdoc/>
        public IUserSettings UserSettings { get; }

        public SettingsService(IModifiableFolder settingsFolder)
        {
            AppSettings = new AppSettings(settingsFolder);
            UserSettings = new UserSettings(settingsFolder);
        }

        /// <inheritdoc/>
        public async Task<bool> LoadAsync(CancellationToken cancellationToken = default)
        {
            var result = true;
            result &= await AppSettings.LoadAsync(cancellationToken);
            result &= await UserSettings.LoadAsync(cancellationToken);

            return result;
        }

        /// <inheritdoc/>
        public async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
        {
            var result = true;
            result &= await AppSettings.SaveAsync(cancellationToken);
            result &= await UserSettings.SaveAsync(cancellationToken);

            return result;
        }
    }
}
