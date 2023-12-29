using System;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;
using Windows.ApplicationModel;
using Windows.System;

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    internal sealed class ApplicationService : BaseApplicationService
    {
        /// <inheritdoc/>
        public override string Platform { get; } =
#if WINDOWS
            "WinUI"
#else
            "Uno"
#endif
            ;

        /// <inheritdoc/>
        public override Version AppVersion
        {
            get
            {
                var packageVersion = Package.Current.Id.Version;
                return new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
            }
        }

        /// <inheritdoc/>
        public override async Task OpenUriAsync(Uri uri)
        {
            await Launcher.LaunchUriAsync(uri);
        }

        /// <inheritdoc/>
        public override Task TryRestartAsync()
        {
#if WINDOWS
            Microsoft.Windows.AppLifecycle.AppInstance.Restart("/RestartCalled");
#endif
            return Task.CompletedTask;
        }
    }
}
