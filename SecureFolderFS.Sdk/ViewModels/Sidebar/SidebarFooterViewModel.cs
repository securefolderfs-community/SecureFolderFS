using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;

namespace SecureFolderFS.Sdk.ViewModels.Sidebar
{
    public sealed class SidebarFooterViewModel : ObservableObject
    {
        private IDialogService DialogService { get; } = Ioc.Default.GetRequiredService<IDialogService>();

        public IAsyncRelayCommand CreateNewVaultCommand { get; }

        public IAsyncRelayCommand OpenSettingsCommand { get; }

        public SidebarFooterViewModel()
        {
            OpenSettingsCommand = new AsyncRelayCommand(OpenSettingsAsync);
            CreateNewVaultCommand = new AsyncRelayCommand(CreateNewVaultAsync);
        }

        private async Task CreateNewVaultAsync()
        {
            await DialogService.ShowDialogAsync(new VaultWizardDialogViewModel());
        }

        private async Task OpenSettingsAsync()
        {
            await DialogService.ShowDialogAsync(new SettingsDialogViewModel());
        }
    }
}
