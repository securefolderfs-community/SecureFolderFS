using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.UserControls.InfoBars
{
    internal sealed class DokanyInfoBar : InfoBarViewModel
    {
        private IApplicationService ApplicationService { get; } = Ioc.Default.GetRequiredService<IApplicationService>();

        public IAsyncRelayCommand OpenWebsiteCommand { get; }

        public DokanyInfoBar()
        {
            OpenWebsiteCommand = new AsyncRelayCommand(OpenWebsiteAsync);
        }

        private Task OpenWebsiteAsync()
        {
            return ApplicationService.OpenUriAsync(new Uri(Sdk.Constants.FileSystems.DOKANY_EXTERNAL_LINK));
        }
    }
}
