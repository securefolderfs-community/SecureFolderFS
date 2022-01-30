using CommunityToolkit.Mvvm.Messaging.Messages;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.ViewModels.Pages;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class NavigationRequestedMessage : ValueChangedMessage<BasePageViewModel?>
    {
        public VaultModel VaultModel { get; }

        public NavigationRequestedMessage(VaultModel vaultModel)
            : this(vaultModel, null)
        {
            this.VaultModel = vaultModel;
        }

        public NavigationRequestedMessage(VaultModel vaultModel, BasePageViewModel? value)
            : base(value)
        {
            this.VaultModel = vaultModel;
        }
    }
}
