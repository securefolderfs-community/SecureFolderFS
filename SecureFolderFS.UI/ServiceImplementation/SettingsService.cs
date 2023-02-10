using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.UI.ServiceImplementation.Settings;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="ISettingsService"/>
    public sealed class SettingsService : ISettingsService
    {
        /// <inheritdoc/>
        public IUserSettings UserSettings { get; }

        /// <inheritdoc/>
        public IApplicationSettings ApplicationSettings { get; }

        public SettingsService(IModifiableFolder settingsFolder)
        {
            UserSettings = new UserSettings(settingsFolder);
            ApplicationSettings = new ApplicationSettings(settingsFolder);
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await Task.WhenAll(ApplicationSettings.LoadAsync(cancellationToken), UserSettings.LoadAsync(cancellationToken));
        }
    }
}
