using System.Reflection;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;
using Windows.System;

namespace SecureFolderFS.Uno.Skia.Gtk.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    internal sealed class SkiaApplicationService : BaseApplicationService
    {
        /// <inheritdoc/>
        public override string Platform { get; } = "Skia Gtk - Uno";

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