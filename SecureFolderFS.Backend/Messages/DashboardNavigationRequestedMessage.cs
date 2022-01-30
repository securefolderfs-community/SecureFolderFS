using CommunityToolkit.Mvvm.Messaging.Messages;
using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.ViewModels.Pages.DashboardPages;

#nullable enable

namespace SecureFolderFS.Backend.Messages
{
    public sealed class DashboardNavigationRequestedMessage : ValueChangedMessage<BaseDashboardPageViewModel?>
    {
        public string? From { get; init; }

        public VaultDashboardPageType VaultDashboardPageType { get; }

        public UnlockedVaultModel UnlockedVaultModel { get; }

        public DashboardNavigationRequestedMessage(VaultDashboardPageType vaultDashboardPageType, UnlockedVaultModel unlockedVaultModel)
            : this(vaultDashboardPageType, unlockedVaultModel, null)
        {
        }

        public DashboardNavigationRequestedMessage(VaultDashboardPageType vaultDashboardPageType, UnlockedVaultModel unlockedVaultModel, BaseDashboardPageViewModel? value)
            : base(value)
        {
            this.VaultDashboardPageType = vaultDashboardPageType;
            this.UnlockedVaultModel = unlockedVaultModel;
        }
    }
}
