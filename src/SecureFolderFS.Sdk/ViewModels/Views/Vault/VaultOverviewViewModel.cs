using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Contexts;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Inject<ISettingsService>, Inject<IFileExplorerService>]
    [Bindable(true)]
    public sealed partial class VaultOverviewViewModel : BaseDesignationViewModel, IUnlockedViewContext, IAsyncInitialize, IDisposable
    {
        [ObservableProperty] private WidgetsListViewModel _WidgetsViewModel;
        [ObservableProperty] private VaultControlsViewModel _VaultControlsViewModel;

        /// <inheritdoc/>
        public UnlockedVaultViewModel UnlockedVaultViewModel { get; }

        /// <inheritdoc/>
        public VaultViewModel VaultViewModel => UnlockedVaultViewModel.VaultViewModel;

        public VaultOverviewViewModel(UnlockedVaultViewModel unlockedVaultViewModel, VaultControlsViewModel vaultControlsViewModel, WidgetsListViewModel widgetsViewModel)
        {
            ServiceProvider = DI.Default;
            UnlockedVaultViewModel = unlockedVaultViewModel;
            WidgetsViewModel = widgetsViewModel;
            VaultControlsViewModel = vaultControlsViewModel;
            Title = unlockedVaultViewModel.VaultViewModel.VaultName;
            VaultViewModel.PropertyChanged += VaultViewModel_PropertyChanged;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (SettingsService.UserSettings.OpenFolderOnUnlock)
                _ = FileExplorerService.TryOpenInFileExplorerAsync(UnlockedVaultViewModel.StorageRoot.Inner, cancellationToken);

            await WidgetsViewModel.InitAsync(cancellationToken);
        }

        private void VaultViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(VaultViewModel.VaultName))
                return;

            Title = VaultViewModel.VaultName;
        }


        /// <inheritdoc/>
        public void Dispose()
        {
            WidgetsViewModel.Dispose();
            VaultViewModel.PropertyChanged -= VaultViewModel_PropertyChanged;
            VaultControlsViewModel.Dispose();
        }
    }
}
