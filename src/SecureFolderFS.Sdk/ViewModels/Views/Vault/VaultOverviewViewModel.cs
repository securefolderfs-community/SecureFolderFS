using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets;
using SecureFolderFS.Shared;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Inject<ISettingsService>, Inject<IFileExplorerService>]
    [Bindable(true)]
    public sealed partial class VaultOverviewViewModel : BaseDashboardViewModel
    {
        [ObservableProperty] private WidgetsListViewModel _WidgetsViewModel;
        [ObservableProperty] private VaultControlsViewModel _VaultControlsViewModel;

        public VaultOverviewViewModel(UnlockedVaultViewModel unlockedVaultViewModel, VaultControlsViewModel vaultControlsViewModel, WidgetsListViewModel widgetsViewModel)
            : base(unlockedVaultViewModel)
        {
            ServiceProvider = DI.Default;
            WidgetsViewModel = widgetsViewModel;
            VaultControlsViewModel = vaultControlsViewModel;
            Title = UnlockedVaultViewModel.VaultModel.VaultName;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (SettingsService.UserSettings.OpenFolderOnUnlock)
                _ = FileExplorerService.TryOpenInFileExplorerAsync(UnlockedVaultViewModel.StorageRoot, cancellationToken);

            await WidgetsViewModel.InitAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            WidgetsViewModel.Dispose();
        }
    }
}
