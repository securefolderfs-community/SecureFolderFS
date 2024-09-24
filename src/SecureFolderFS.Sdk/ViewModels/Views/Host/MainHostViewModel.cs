using System;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.VaultList;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Views.Host
{
    [Inject<INavigationService>(Visibility = "public"), Inject<IOverlayService>, Inject<ISettingsService>]
    [Bindable(true)]
    public sealed partial class MainHostViewModel : ObservableObject, IViewDesignation, IAsyncInitialize
    {
        [ObservableProperty] private string? _Title;

        public VaultListViewModel VaultListViewModel { get; }

        public MainHostViewModel(IVaultCollectionModel vaultCollectionModel)
        {
            ServiceProvider = DI.Default;
            VaultListViewModel = new(vaultCollectionModel);
            vaultCollectionModel.CollectionChanged += VaultCollectionModel_CollectionChanged;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return VaultListViewModel.InitAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void OnAppearing()
        {
        }

        /// <inheritdoc/>
        public void OnDisappearing()
        {
        }
        
        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task OpenSettingsAsync()
        {
            await OverlayService.ShowAsync(SettingsOverlayViewModel.Instance);
            await SettingsService.TrySaveAsync();
        }
        
        private void VaultCollectionModel_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove when e.OldItems is not null && e.OldItems[0] is IVaultModel vaultModel:
                    if (NavigationService.Views.FirstOrDefault(x => (x as BaseVaultViewModel)?.VaultModel.Equals(vaultModel) ?? false) is not BaseVaultViewModel viewModel)
                        return;

                    NavigationService.Views.Remove(viewModel);
                    viewModel.Dispose();
                    break;
                
                case NotifyCollectionChangedAction.Reset:
                    NavigationService.Views.DisposeElements();
                    NavigationService.Views.Clear();
                    break;
            }
        }
    }
}
