using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Contexts;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Inject<IFileExplorerService>]
    [Bindable(true)]
    public sealed partial class VaultControlsViewModel : ObservableObject, IRecipient<VaultLockRequestedMessage>, IDisposable
    {
        private readonly INavigator _dashboardNavigator;
        private readonly INavigationService _vaultNavigation;
        private readonly UnlockedVaultViewModel _unlockedVaultViewModel;
        private readonly BrowserViewModel? _browserViewModel;
        private VaultPropertiesViewModel? _propertiesViewModel;

        public VaultControlsViewModel(
            INavigationService vaultNavigation,
            INavigator dashboardNavigator,
            UnlockedVaultViewModel unlockedVaultViewModel,
            BrowserViewModel? browserViewModel = null,
            VaultPropertiesViewModel? propertiesViewModel = null)
        {
            ServiceProvider = DI.Default;
            _vaultNavigation = vaultNavigation;
            _dashboardNavigator = dashboardNavigator;
            _unlockedVaultViewModel = unlockedVaultViewModel;
            _browserViewModel = browserViewModel;
            _propertiesViewModel = propertiesViewModel;

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
            await FileExplorerService.TryOpenInFileExplorerAsync(_unlockedVaultViewModel.StorageRoot.VirtualizedRoot, cancellationToken);
        }

        [RelayCommand]
        private async Task BrowseAsync(CancellationToken cancellationToken)
        {
            if (_browserViewModel is not null)
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

            // TODO: On MAUI iOS, after navigating to Browser and back, the current view is set to the BrowserViewModel.
            // Since the back navigation is not being tracked of, the CurrentView is not updated, and thus
            // the navigator only silently navigates assuming that we're on a different/correct view already.
            // The fix would involve keeping track of Shell.Current.Navigated event. This would also require for a custom
            // interface to be added to retrieve a view model from a current view (IWrapper<INotifyPropertyChanged?>)

            // Navigate away
            await _vaultNavigation.ForgetNavigateSpecificViewAsync(loginPageViewModel, x => (x as IVaultViewContext)?.VaultViewModel.VaultModel.Equals(_unlockedVaultViewModel.VaultViewModel.VaultModel) ?? false);
            WeakReferenceMessenger.Default.Send(new VaultLockedMessage(_unlockedVaultViewModel.VaultViewModel.VaultModel));
        }

        [RelayCommand]
        private async Task OpenPropertiesAsync(CancellationToken cancellationToken)
        {
            if (_propertiesViewModel is null)
            {
                _propertiesViewModel = new(_unlockedVaultViewModel, _dashboardNavigator, _vaultNavigation);
                _ = _propertiesViewModel.InitAsync(cancellationToken);
            }

            await _dashboardNavigator.NavigateAsync(_propertiesViewModel);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }
    }
}
