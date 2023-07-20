using SecureFolderFS.AvaloniaUI.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    internal sealed class ApplicationService : BaseApplicationService
    {
        /// <inheritdoc/>
        public override string Platform { get; } = "AvaloniaUI";

        /// <inheritdoc/>
        public override Version AppVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version!;

        /// <inheritdoc/>
        public override Task OpenUriAsync(Uri uri)
        {
            LauncherHelpers.Launch(uri);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override Task TryRestartAsync()
        {
            throw new NotImplementedException();
        }
    }
}