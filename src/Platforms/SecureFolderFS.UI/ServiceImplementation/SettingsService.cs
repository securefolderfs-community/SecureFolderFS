using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Settings;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="ISettingsService"/>
    public sealed class SettingsService : ISettingsService
    {
        /// <inheritdoc/>
        public IAppSettings AppSettings { get; }

        /// <inheritdoc/>
        public IUserSettings UserSettings { get; }

        public SettingsService(IAppSettings appSettings, IUserSettings userSettings)
        {
            AppSettings = appSettings;
            UserSettings = userSettings;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.WhenAll(AppSettings.InitAsync(cancellationToken), UserSettings.InitAsync(cancellationToken));
        }

        /// <inheritdoc/>
        public Task SaveAsync(CancellationToken cancellationToken = default)
        {
            return Task.WhenAll(AppSettings.SaveAsync(cancellationToken), UserSettings.SaveAsync(cancellationToken));
        }
    }
}
