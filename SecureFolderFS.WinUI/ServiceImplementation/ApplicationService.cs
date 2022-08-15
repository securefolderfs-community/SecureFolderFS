using SecureFolderFS.Sdk.Services;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    internal sealed class ApplicationService : IApplicationService
    {
        /// <inheritdoc/>
        public Version GetAppVersion()
        {
            var packageVersion = Package.Current.Id.Version;
            return new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }

        /// <inheritdoc/>
        public async Task OpenUriAsync(Uri uri)
        {
            await Launcher.LaunchUriAsync(uri);
        }
    }
}
