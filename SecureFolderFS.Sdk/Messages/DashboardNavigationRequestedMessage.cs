using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models.Transitions;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Sdk.ViewModels.Pages.DashboardPages;

namespace SecureFolderFS.Sdk.Messages
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
            VaultDashboardPageType = vaultDashboardPageType;
            VaultViewModel = vaultViewModel;
        }
    }
}
