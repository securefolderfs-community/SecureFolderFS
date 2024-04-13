using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using SecureFolderFS.Shared.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Inject<IFileExplorerService>]
    public sealed partial class VaultControlsViewModel : ObservableObject
    {
        private readonly INavigator _navigator;
        private readonly UnlockedVaultViewModel _unlockedVaultViewModel;
        private VaultPropertiesViewModel? _propertiesViewModel;

        public VaultControlsViewModel(INavigator navigator, UnlockedVaultViewModel unlockedVaultViewModel)
        {
            _navigator = navigator;
            _unlockedVaultViewModel = unlockedVaultViewModel;
            ServiceProvider = Ioc.Default;
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task RevealFolderAsync(CancellationToken cancellationToken)
        {
            await FileExplorerService.TryOpenInFileExplorerAsync(_unlockedVaultViewModel.StorageRoot, cancellationToken);
        }

        [RelayCommand]
        private async Task LockVaultAsync()
        {
            // Lock vault
            await _unlockedVaultViewModel.DisposeAsync();

            // Prepare login page
            var loginPageViewModel = new VaultLoginViewModel(_unlockedVaultViewModel.VaultModel);
            _ = loginPageViewModel.InitAsync();

            // Navigate away
            await _navigator.NavigateAsync(loginPageViewModel);
            WeakReferenceMessenger.Default.Send(new VaultLockedMessage(_unlockedVaultViewModel.VaultModel));
        }

        [RelayCommand]
        private async Task OpenPropertiesAsync()
        {
            _propertiesViewModel ??= new(_unlockedVaultViewModel);
            await _navigator.NavigateAsync(_propertiesViewModel);
        }
    }
}
