using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets;
using SecureFolderFS.Sdk.ViewModels.Vault;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard
{
    [Inject<ISettingsService>, Inject<IFileExplorerService>]
    public sealed partial class VaultOverviewViewModel : BaseDashboardViewModel
    {
        [ObservableProperty] private WidgetsListViewModel _WidgetsViewModel;
        [ObservableProperty] private VaultControlsViewModel _VaultControlsViewModel;

        public VaultOverviewViewModel(UnlockedVaultViewModel unlockedVaultViewModel, VaultControlsViewModel vaultControlsViewModel, WidgetsListViewModel widgetsViewModel)
            : base(unlockedVaultViewModel)
        {
            ServiceProvider = Ioc.Default;
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
