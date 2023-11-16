using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets;
using SecureFolderFS.Sdk.ViewModels.Vault;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard
{
    [Inject<ISettingsService>, Inject<IFileExplorerService>]
    public sealed partial class VaultOverviewPageViewModel : BaseDashboardPageViewModel
    {
        public WidgetsListViewModel WidgetsViewModel { get; }

        public VaultControlsViewModel VaultControlsViewModel { get; }

        /// <inheritdoc/>
        public override string PageName => UnlockedVaultViewModel.VaultViewModel.VaultModel.VaultName;

        public VaultOverviewPageViewModel(UnlockedVaultViewModel unlockedVaultViewModel, VaultControlsViewModel vaultControlsViewModel, INavigationService dashboardNavigationService)
            : base(unlockedVaultViewModel, dashboardNavigationService)
        {
            ServiceProvider = Ioc.Default;
            WidgetsViewModel = new(unlockedVaultViewModel.VaultLifeTimeModel, unlockedVaultViewModel.VaultViewModel.WidgetsContextModel);
            VaultControlsViewModel = vaultControlsViewModel;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (SettingsService.UserSettings.OpenFolderOnUnlock && UnlockedVaultViewModel.VaultLifeTimeModel.RootFolder is ILocatableFolder rootFolder)
                _ = FileExplorerService.OpenInFileExplorerAsync(rootFolder, cancellationToken);

            await WidgetsViewModel.InitAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public override void OnNavigatingFrom()
        {
            WidgetsViewModel.Dispose();
        }
    }
}
