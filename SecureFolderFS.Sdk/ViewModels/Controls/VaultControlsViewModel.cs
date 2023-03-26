using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard;
using SecureFolderFS.Sdk.ViewModels.Vault;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    public sealed partial class VaultControlsViewModel : ObservableObject
    {
        private readonly UnlockedVaultViewModel _unlockedVaultViewModel;
        private readonly IStateNavigationModel _dashboardNavigationModel;
        private readonly INavigationModel _navigationModel;

        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        public VaultControlsViewModel(UnlockedVaultViewModel unlockedVaultViewModel, IStateNavigationModel dashboardNavigationModel, INavigationModel navigationModel)
        {
            _unlockedVaultViewModel = unlockedVaultViewModel;
            _dashboardNavigationModel = dashboardNavigationModel;
            _navigationModel = navigationModel;
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task ShowInFileExplorerAsync(CancellationToken cancellationToken)
        {
            if (_unlockedVaultViewModel.UnlockedVaultModel.RootFolder is not ILocatableFolder rootFolder)
                return;

            await FileExplorerService.OpenInFileExplorerAsync(rootFolder, cancellationToken);
        }

        [RelayCommand]
        private async Task LockVaultAsync(CancellationToken cancellationToken)
        {
            await _unlockedVaultViewModel.UnlockedVaultModel.LockAsync();

            var loginPageViewModel = new VaultLoginPageViewModel(_unlockedVaultViewModel.VaultViewModel);
            _ = loginPageViewModel.InitAsync(cancellationToken);

            WeakReferenceMessenger.Default.Send(new VaultLockedMessage(_unlockedVaultViewModel.VaultViewModel.VaultModel));
            WeakReferenceMessenger.Default.Send(new NavigationMessage(loginPageViewModel));
        }

        [RelayCommand]
        private async Task OpenPropertiesAsync()
        {
            var target = _dashboardNavigationModel.Targets.FirstOrDefault(x => x is VaultPropertiesPageViewModel)
                         ?? new VaultPropertiesPageViewModel(_unlockedVaultViewModel, _dashboardNavigationModel);

            await _navigationModel.NavigateAsync(target);
        }
    }
}
