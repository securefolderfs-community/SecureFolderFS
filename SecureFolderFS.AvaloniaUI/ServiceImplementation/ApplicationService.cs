using SecureFolderFS.AvaloniaUI.Helpers;
using SecureFolderFS.Sdk.AppModels;
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
        public override AppVersion GetAppVersion()
        {
            return new(Assembly.GetExecutingAssembly().GetName().Version!, "AvaloniaUI");
        }

        /// <inheritdoc/>
        public override Task OpenUriAsync(Uri uri)
        {
            LauncherHelpers.Launch(uri);
            return Task.CompletedTask;
        }
    }
}