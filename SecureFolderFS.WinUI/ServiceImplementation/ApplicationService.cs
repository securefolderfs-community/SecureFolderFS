using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    internal sealed class ApplicationService : BaseApplicationService
    {
        /// <inheritdoc/>
        public override string Platform { get; } = "WinUI";

        /// <inheritdoc/>
        public override AppVersion GetAppVersion()
        {
            var packageVersion = Package.Current.Id.Version;
            var version = new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);

            return new(version, Platform);
        }

        /// <inheritdoc/>
        public override async Task OpenUriAsync(Uri uri)
        {
            await Launcher.LaunchUriAsync(uri);
        }
    }
}
