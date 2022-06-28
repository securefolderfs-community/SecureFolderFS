using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;

namespace SecureFolderFS.Sdk.ViewModels.Settings.InfoBars
{
    public sealed class DokanyInfoBarViewModel : InfoBarViewModel
    {
        private IApplicationService ApplicationService { get; } = Ioc.Default.GetRequiredService<IApplicationService>();

        public IAsyncRelayCommand OpenWebsiteCommand { get; }

        public DokanyInfoBarViewModel()
        {
            OpenWebsiteCommand = new AsyncRelayCommand(OpenWebsiteAsync);
        }

        private Task OpenWebsiteAsync()
        {
            return ApplicationService.OpenUriAsync(new Uri(Constants.FileSystems.DOKANY_EXTERNAL_LINK));
        }
    }
}
