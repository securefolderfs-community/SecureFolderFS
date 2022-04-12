using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Backend.Services;

namespace SecureFolderFS.Backend.ViewModels.Dialogs
{
    public sealed class DokanyDialogViewModel : BaseDialogViewModel
    {
        private IApplicationService ApplicationService { get; } = Ioc.Default.GetRequiredService<IApplicationService>();

        private string? _ErrorText;
        public string? ErrorText
        {
            get => _ErrorText;
            set => SetProperty(ref _ErrorText, value);
        }

        public DokanyDialogViewModel()
        {
            PrimaryButtonClickCommand = new AsyncRelayCommand(PrimaryButtonClick);
            SecondaryButtonClickCommand = new RelayCommand(SecondaryButtonClick);
        }

        private async Task PrimaryButtonClick()
        {
            await ApplicationService.OpenUriAsync(new Uri("https://github.com/dokan-dev/dokany/releases/tag/v1.5.1.1000"));
        }

        private void SecondaryButtonClick()
        {
            ApplicationService.CloseApplication();
        }
    }
}
