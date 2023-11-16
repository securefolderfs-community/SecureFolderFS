﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Inject<IFileExplorerService>]
    public sealed partial class VaultControlsViewModel : ObservableObject
    {
        private readonly UnlockedVaultViewModel _unlockedVaultViewModel;
        private readonly INavigationService _dashboardNavigationService;
        private readonly INavigationService _navigationService;

        public VaultControlsViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigationService dashboardNavigationService, INavigationService navigationService)
        {
            ServiceProvider = Ioc.Default;
            _unlockedVaultViewModel = unlockedVaultViewModel;
            _dashboardNavigationService = dashboardNavigationService;
            _navigationService = navigationService;
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task RevealFolderAsync(CancellationToken cancellationToken)
        {
            if (_unlockedVaultViewModel.VaultLifeTimeModel.RootFolder is not ILocatableFolder rootFolder)
                return;

            await FileExplorerService.OpenInFileExplorerAsync(rootFolder, cancellationToken);
        }

        [RelayCommand]
        private async Task LockVaultAsync()
        {
            // Lock vault
            await _unlockedVaultViewModel.VaultLifeTimeModel.DisposeAsync();

            // Navigate away
            var loginPageViewModel = new VaultLoginPageViewModel(_unlockedVaultViewModel.VaultViewModel, _navigationService);
            await _navigationService.TryNavigateAndForgetAsync(loginPageViewModel);

            WeakReferenceMessenger.Default.Send(new VaultLockedMessage(_unlockedVaultViewModel.VaultViewModel.VaultModel));
        }

        [RelayCommand]
        private async Task OpenPropertiesAsync()
        {
            await _dashboardNavigationService.TryNavigateAsync(() => new VaultPropertiesPageViewModel(_unlockedVaultViewModel, _dashboardNavigationService));
        }
    }
}
