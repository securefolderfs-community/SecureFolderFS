using Microsoft.UI.Xaml;
using SecureFolderFS.Backend.Services;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;

#nullable enable

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    internal sealed class ApplicationService : IApplicationService
    {
        public Version GetAppVersion()
        {
            var packageVersion = Package.Current.Id.Version;

            return new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }

        public void CloseApplication()
        {
            Application.Current.Exit();
        }

        public async Task OpenUriAsync(Uri uri)
        {
            await Launcher.LaunchUriAsync(uri);
        }

        public async Task OpenAppFolderAsync()
        {
            await Launcher.LaunchFolderAsync(ApplicationData.Current.LocalFolder);
        }
    }
}
