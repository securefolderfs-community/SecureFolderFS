using SecureFolderFS.Sdk.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System;
using SecureFolderFS.Sdk.AppModels;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    internal sealed class ApplicationService : IApplicationService
    {
        /// <inheritdoc/>
        public AppVersion GetAppVersion()
        {
            var packageVersion = Package.Current.Id.Version;
            return new(new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision), "WinUI");
        }

        /// <inheritdoc/>
        public async Task OpenUriAsync(Uri uri)
        {
            await Launcher.LaunchUriAsync(uri);
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<LicenseModel> GetLicensesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
