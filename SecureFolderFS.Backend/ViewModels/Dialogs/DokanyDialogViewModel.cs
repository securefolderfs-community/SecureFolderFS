using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Backend.Services;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Dialogs
{
    public sealed class DokanyDialogViewModel : ObservableObject
    {
        private IApplicationService ApplicationService { get; } = Ioc.Default.GetRequiredService<IApplicationService>();

        private string? _ErrorText;
        public string? ErrorText
        {
            get => _ErrorText;
            set => SetProperty(ref _ErrorText, value);
        }

        public IAsyncRelayCommand OpenInstallationWebsiteCommand { get; }

        public IRelayCommand CloseApplicationCommand { get; }

        public DokanyDialogViewModel()
        {
            OpenInstallationWebsiteCommand = new AsyncRelayCommand(OpenInstallationWebsite);
            CloseApplicationCommand = new RelayCommand(CloseApplication);
        }

        private async Task OpenInstallationWebsite()
        {
            await ApplicationService.OpenUriAsync(new Uri("https://github.com/dokan-dev/dokany/releases/tag/v1.5.1.1000"));
        }

        private void CloseApplication()
        {
            ApplicationService.CloseApplication();
        }
    }
}
