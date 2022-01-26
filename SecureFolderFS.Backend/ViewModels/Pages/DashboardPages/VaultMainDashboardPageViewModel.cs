using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.Services;
using SecureFolderFS.Backend.ViewModels.Dashboard;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Pages.DashboardPages
{
    public sealed class VaultMainDashboardPageViewModel : BaseDashboardPageViewModel
    {
        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        public VaultHealthViewModel VaultHealthViewModel { get; }

        public GraphWidgetControlViewModel ReadGraphViewModel { get; }

        public GraphWidgetControlViewModel WriteGraphViewModel { get; }

        public IAsyncRelayCommand ShowInFileExplorerCommand { get; }

        public IRelayCommand LockVaultCommand { get; }

        public IRelayCommand OpenVaultPropertiesCommand { get; }

        public VaultMainDashboardPageViewModel(VaultModel vaultModel)
            : base(vaultModel)
        {
            VaultHealthViewModel = new();
            ReadGraphViewModel = new() { GraphSubheader = "0mb/s" };
            WriteGraphViewModel = new() { GraphSubheader = "0mb/s" };

            ShowInFileExplorerCommand = new AsyncRelayCommand(ShowInFileExplorer);
            LockVaultCommand = new RelayCommand(LockVault);
            OpenVaultPropertiesCommand = new RelayCommand(OpenVaultProperties);
        }

        private async Task ShowInFileExplorer()
        {
            // await FileExplorerService.OpenPathInFileExplorerAsync(""); // TODO Vault opened path
        }

        private void LockVault()
        {

        }

        private void OpenVaultProperties()
        {
            // TODO: Navigate to vault properties page (VaultDashboardPropertiesPage)
        }
    }
}
