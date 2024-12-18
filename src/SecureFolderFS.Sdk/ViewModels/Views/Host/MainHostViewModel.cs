using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.VaultList;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Host
{
    [Inject<INavigationService>(Visibility = "public"), Inject<IOverlayService>, Inject<ISettingsService>]
    [Bindable(true)]
    public sealed partial class MainHostViewModel : BaseDesignationViewModel, IAsyncInitialize, IDisposable
    {
        private readonly IVaultCollectionModel _vaultCollectionModel;
        private readonly ISystemMonitorModel _systemMonitorModel;

        public VaultListViewModel VaultListViewModel { get; }

        public MainHostViewModel(IVaultCollectionModel vaultCollectionModel)
        {
            ServiceProvider = DI.Default;
            _vaultCollectionModel = vaultCollectionModel;
            _systemMonitorModel = new SystemMonitorModel(vaultCollectionModel);
            Title = "MyVaults".ToLocalized();
            VaultListViewModel = new(vaultCollectionModel);
            _vaultCollectionModel.CollectionChanged += VaultCollectionModel_CollectionChanged;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await VaultListViewModel.InitAsync(cancellationToken);
            await _systemMonitorModel.InitAsync(cancellationToken);
        }
        
        [RelayCommand]
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
                    if (NavigationService.Views.FirstOrDefault(x => (x as BaseVaultViewModel)?.VaultViewModel.VaultModel.Equals(vaultModel) ?? false) is not BaseVaultViewModel viewModel)
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

        /// <inheritdoc/>
        public void Dispose()
        {
            _vaultCollectionModel.CollectionChanged += VaultCollectionModel_CollectionChanged;
            _systemMonitorModel.Dispose();
        }
    }
}
