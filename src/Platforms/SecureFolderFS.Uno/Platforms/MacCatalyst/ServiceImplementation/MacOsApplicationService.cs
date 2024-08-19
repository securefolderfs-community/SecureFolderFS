using System;
using System.Reflection;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;
using Windows.System;

namespace SecureFolderFS.Uno.Platforms.MacCatalyst.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    internal sealed class MacOsApplicationService : BaseApplicationService
    {
        /// <inheritdoc/>
        public override string Platform { get; } = "Mac Catalyst - Uno";

        /// <inheritdoc/>
        public override Version AppVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version!;
            }
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
