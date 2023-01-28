using System;
using System.Reflection;
using System.Threading.Tasks;
using SecureFolderFS.AvaloniaUI.Helpers;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    internal sealed class ApplicationService : IApplicationService
    {
        /// <inheritdoc/>
        public AppVersion GetAppVersion()
        {
            return new(Assembly.GetExecutingAssembly().GetName().Version!, "AvaloniaUI");
        }

        /// <inheritdoc/>
        public Task OpenUriAsync(Uri uri)
        {
            LauncherHelper.Launch(uri);
            return Task.CompletedTask;
        }
    }
}