using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.Services;
using SecureFolderFS.Backend.ViewModels.Dashboard;
using SecureFolderFS.Backend.ViewModels.Dashboard.Navigation;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Pages.DashboardPages
{
    public sealed class VaultMainDashboardPageViewModel : BaseDashboardPageViewModel
    {
        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        public VaultHealthViewModel VaultHealthViewModel { get; }

        public GraphWidgetControlViewModel ReadGraphViewModel { get; }

        public GraphWidgetControlViewModel WriteGraphViewModel { get; }

        public VaultIoSpeedReporterModel VaultIoSpeedReporterModel { get; }

        public override int Index { get; }

        public override Action<DashboardNavigationItemViewModel?> NavigationAction { get; }

        public override string SectionName { get; }

        public IAsyncRelayCommand ShowInFileExplorerCommand { get; }

        public IRelayCommand LockVaultCommand { get; }

        public IRelayCommand OpenVaultPropertiesCommand { get; }

        public VaultMainDashboardPageViewModel(UnlockedVaultModel unlockedVaultModel)
            : base(unlockedVaultModel)
        {
            this.VaultHealthViewModel = new();
            this.ReadGraphViewModel = new() { GraphSubheader = "0mb/s" };
            this.WriteGraphViewModel = new() { GraphSubheader = "0mb/s" };
            this.VaultIoSpeedReporterModel = new()
            {
                ReadGraphViewModel = this.ReadGraphViewModel,
                WriteGraphViewModel = this.WriteGraphViewModel
            };

            this.Index = 0;
            this.NavigationAction = (first) => WeakReferenceMessenger.Default.Send(new DashboardNavigationRequestedMessage(VaultDashboardPageType.MainDashboardPage, unlockedVaultModel) { From = first?.SectionName });
            this.SectionName = unlockedVaultModel.VaultModel!.VaultName!;

            this.VaultIoSpeedReporterModel.Start();

            ShowInFileExplorerCommand = new AsyncRelayCommand(ShowInFileExplorer);
            LockVaultCommand = new RelayCommand(LockVault);
            OpenVaultPropertiesCommand = new RelayCommand(OpenVaultProperties);
        }

        private async Task ShowInFileExplorer()
        {
            if (UnlockedVaultModel.VaultInstance != null)
            {
                await FileExplorerService.OpenPathInFileExplorerAsync(UnlockedVaultModel.VaultInstance.SecureFolderFSInstance.MountLocation);
            }
        }

        private void LockVault()
        {
            UnlockedVaultModel.VaultInstance?.Dispose();
            WeakReferenceMessenger.Default.Send(new LockVaultRequestedMessage(UnlockedVaultModel.VaultModel));
        }

        private void OpenVaultProperties()
        {
            WeakReferenceMessenger.Default.Send(new DashboardNavigationRequestedMessage(VaultDashboardPageType.DashboardPropertiesPage, UnlockedVaultModel));
        }
    }
}
