using System;
using System.Threading.Tasks;
using Windows.System;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Uno.Platforms.MacCatalyst.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    internal sealed class MacOsApplicationService : BaseApplicationService
    {
        /// <inheritdoc/>
        public override bool IsDesktop { get; } = true;
        
        /// <inheritdoc/>
        public override string Platform { get; } = "Mac Catalyst - Uno";

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
