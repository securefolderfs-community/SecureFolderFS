using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.UI.ServiceImplementation.Settings;

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
        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.WhenAll(AppSettings.LoadAsync(cancellationToken), UserSettings.LoadAsync(cancellationToken));
        }

        /// <inheritdoc/>
        public Task SaveAsync(CancellationToken cancellationToken = default)
        {
            return Task.WhenAll(AppSettings.SaveAsync(cancellationToken), UserSettings.SaveAsync(cancellationToken));
        }
    }
}
