using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    public sealed partial class VaultControlsViewModel : ObservableObject
    {
        private readonly UnlockedVaultViewModel _unlockedVaultViewModel;
        private readonly INavigationService _dashboardNavigationService;
        private readonly INavigationService _navigationService;

        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        public VaultControlsViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigationService dashboardNavigationService, INavigationService navigationService)
        {
            _unlockedVaultViewModel = unlockedVaultViewModel;
            _dashboardNavigationService = dashboardNavigationService;
            _navigationService = navigationService;
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task RevealFolderAsync(CancellationToken cancellationToken)
        {
            if (_unlockedVaultViewModel.UnlockedVaultModel.RootFolder is not ILocatableFolder rootFolder)
                return;

            await FileExplorerService.OpenInFileExplorerAsync(rootFolder, cancellationToken);
        }

        [RelayCommand]
        private async Task LockVaultAsync(CancellationToken cancellationToken)
        {
            await _unlockedVaultViewModel.UnlockedVaultModel.LockAsync();

            var loginPageViewModel = new VaultLoginPageViewModel(_unlockedVaultViewModel.VaultViewModel, null); // TODO(r)
            _ = loginPageViewModel.InitAsync(cancellationToken);

            WeakReferenceMessenger.Default.Send(new VaultLockedMessage(_unlockedVaultViewModel.VaultViewModel.VaultModel));
            WeakReferenceMessenger.Default.Send(new NavigationMessage(loginPageViewModel));
        }

        [RelayCommand]
        private async Task OpenPropertiesAsync()
        {
            await _dashboardNavigationService.TryNavigateAsync(() => new VaultPropertiesPageViewModel(_unlockedVaultViewModel, _dashboardNavigationService));
        }
    }
}
