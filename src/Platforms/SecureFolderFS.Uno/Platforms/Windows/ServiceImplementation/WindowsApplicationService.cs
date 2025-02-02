using System;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;
using Windows.System;

namespace SecureFolderFS.Uno.Platforms.Windows.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    internal sealed class WindowsApplicationService : BaseApplicationService
    {
        /// <inheritdoc/>
        public override bool IsDesktop { get; } = true;
        
        /// <inheritdoc/>
        public override string Platform { get; } = "WinUI";

        /// <inheritdoc/>
        public override Version AppVersion
        {
            get
            {
#if UNPACKAGED
                return base.AppVersion;
#else
                var packageVersion = global::Windows.ApplicationModel.Package.Current.Id.Version;
                return new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
#endif
            }
        }

        /// <inheritdoc/>
        public override string GetSystemVersion()
        {
            return Environment.OSVersion.VersionString;
        }

        /// <inheritdoc/>
        public override async Task OpenUriAsync(Uri uri)
        {
            await Launcher.LaunchUriAsync(uri);
        }

        /// <inheritdoc/>
        public override Task TryRestartAsync()
        {
            Microsoft.Windows.AppLifecycle.AppInstance.Restart("/RestartCalled");
            return Task.CompletedTask;
        }
    }
}
