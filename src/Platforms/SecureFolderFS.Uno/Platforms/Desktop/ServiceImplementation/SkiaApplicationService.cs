using System;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;
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
        public override string Platform { get; } = "Skia X11 - Uno";

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

        public override Task TryRestartAsync()
        {
            return Task.CompletedTask;
        }
    }
}
