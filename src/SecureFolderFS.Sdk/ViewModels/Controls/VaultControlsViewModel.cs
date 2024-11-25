using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Browser;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Inject<IFileExplorerService>]
    [Bindable(true)]
    public sealed partial class VaultControlsViewModel : ObservableObject, IRecipient<VaultLockRequestedMessage>
    {
        private readonly INavigator _dashboardNavigator;
        private readonly INavigationService _vaultNavigation;
        private readonly UnlockedVaultViewModel _unlockedVaultViewModel;
        private readonly BrowserViewModel _browserViewModel;
        private VaultPropertiesViewModel? _propertiesViewModel;

        public VaultControlsViewModel(INavigationService vaultNavigation, INavigator dashboardNavigator, UnlockedVaultViewModel unlockedVaultViewModel, VaultPropertiesViewModel? propertiesViewModel = null)
        {
            ServiceProvider = DI.Default;
            _vaultNavigation = vaultNavigation;
            _dashboardNavigator = dashboardNavigator;
            _unlockedVaultViewModel = unlockedVaultViewModel;
            _propertiesViewModel = propertiesViewModel;

            var folder = unlockedVaultViewModel.StorageRoot.Inner;
            var folderViewModel = new FolderViewModel(folder, null).WithInitAsync();
            _browserViewModel = new(folderViewModel);

            WeakReferenceMessenger.Default.Register(this);
        }

        /// <inheritdoc/>
        public async void Receive(VaultLockRequestedMessage message)
        {
            if (!message.VaultModel.Equals(_unlockedVaultViewModel.VaultViewModel.VaultModel))
                return;

            await LockVaultAsync();
        }

        [RelayCommand]
        private async Task RevealFolderAsync(CancellationToken cancellationToken)
        {
            await FileExplorerService.TryOpenInFileExplorerAsync(_unlockedVaultViewModel.StorageRoot.Inner, cancellationToken);
        }

        [RelayCommand]
        private async Task BrowseAsync(CancellationToken cancellationToken)
        {
            await _dashboardNavigator.NavigateAsync(_browserViewModel);
        }

        [RelayCommand]
        private async Task LockVaultAsync()
        {
            // Lock vault
            await _unlockedVaultViewModel.DisposeAsync();

            // Prepare login page
            var loginPageViewModel = new VaultLoginViewModel(_unlockedVaultViewModel.VaultViewModel, _vaultNavigation);
            _ = loginPageViewModel.InitAsync();

            // Navigate away
            await _vaultNavigation.TryNavigateAndForgetAsync(loginPageViewModel);
            WeakReferenceMessenger.Default.Send(new VaultLockedMessage(_unlockedVaultViewModel.VaultViewModel.VaultModel));
        }

        [RelayCommand]
        private async Task OpenPropertiesAsync(CancellationToken cancellationToken)
        {
            if (_propertiesViewModel is null)
            {
                _propertiesViewModel = new(_unlockedVaultViewModel);
                await _propertiesViewModel.InitAsync(cancellationToken);
            }

            await _dashboardNavigator.NavigateAsync(_propertiesViewModel);
        }
    }
}
