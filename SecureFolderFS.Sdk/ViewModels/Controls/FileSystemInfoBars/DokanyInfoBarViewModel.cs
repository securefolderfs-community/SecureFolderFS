using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Sdk.ViewModels.Controls.FileSystemInfoBars
{
    public sealed class DokanyInfoBarViewModel : InfoBarViewModel
    {
        private IApplicationService ApplicationService { get; } = Ioc.Default.GetRequiredService<IApplicationService>();

        public IAsyncRelayCommand OpenInstallationLinkCommand { get; }

        public DokanyInfoBarViewModel()
        {
            OpenInstallationLinkCommand = new AsyncRelayCommand(OpenInstallationLink);
        }

        private async Task OpenInstallationLink()
        {
            await ApplicationService.OpenUriAsync(new Uri(Constants.FileSystems.DOKANY_EXTERNAL_LINK));
        }
    }
}
