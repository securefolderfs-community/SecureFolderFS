using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Backend.Models.Transitions;
using SecureFolderFS.Backend.ViewModels;
using SecureFolderFS.Backend.ViewModels.Pages.DashboardPages;

#nullable enable

namespace SecureFolderFS.Backend.Messages
{
    public sealed class DashboardNavigationRequestedMessage : ValueMessage<BaseDashboardPageViewModel?>
    {
        public TransitionModel? Transition { get; init; }

        public VaultDashboardPageType VaultDashboardPageType { get; }

        public VaultViewModel VaultViewModel { get; }

        public DashboardNavigationRequestedMessage(VaultDashboardPageType vaultDashboardPageType, VaultViewModel vaultViewModel)
            : this(vaultDashboardPageType, vaultViewModel, null)
        {
        }

        public DashboardNavigationRequestedMessage(VaultDashboardPageType vaultDashboardPageType, VaultViewModel vaultViewModel, BaseDashboardPageViewModel? value)
            : base(value)
        {
            this.VaultDashboardPageType = vaultDashboardPageType;
            this.VaultViewModel = vaultViewModel;
        }
    }
}
