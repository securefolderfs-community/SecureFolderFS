using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.ServiceImplementation;
using Windows.System;

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    internal sealed class SkiaApplicationService : BaseApplicationService
    {
        /// <inheritdoc/>
        public override bool IsDesktop { get; } = true;

        /// <inheritdoc/>
        public override string Platform { get; } =
#if __UNO_SKIA_MACOS__
            "Skia MacOS - Uno";
#else
            "Skia X11 - Uno";
#endif

        /// <inheritdoc/>
        public override Version AppVersion
        {
            get
            {
                var packageVersion = Windows.ApplicationModel.Package.Current.Id.Version;
                return new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
            }
        }

        /// <inheritdoc/>
        public override string GetSystemVersion()
        {
            return 0 switch
            {
                _ when OperatingSystem.IsMacOS() => $"MacOS {Environment.OSVersion.VersionString}",
                _ when OperatingSystem.IsLinux() => $"Linux {Environment.OSVersion.VersionString}",
                _ => Environment.OSVersion.VersionString
            };
        }

        /// <inheritdoc/>
        public override async Task OpenUriAsync(Uri uri)
        {
            await Launcher.LaunchUriAsync(uri);
        }

        /// <inheritdoc/>
        public override Task<bool> TryOpenWindowAsync(IViewable viewable, CancellationToken cancellationToken = default)
        {
            return App.Instance?.OpenViewWindowAsync(viewable) ?? Task.FromResult(false);
        }

        public override Task TryRestartAsync()
        {
            return Task.CompletedTask;
        }
    }
}
