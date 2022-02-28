using Microsoft.UI.Xaml;
using SecureFolderFS.Backend.Services;
using System;
using System.Threading.Tasks;
using Windows.System;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    internal sealed class ApplicationService : IApplicationService
    {
        public void CloseApplication()
        {
            Application.Current.Exit();
        }

        public async Task OpenUriAsync(Uri uri)
        {
            await Launcher.LaunchUriAsync(uri);
        }
    }
}
