using CommunityToolkit.Mvvm.Messaging.Messages;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.Models.Transitions;
using SecureFolderFS.Backend.ViewModels.Pages;

#nullable enable

namespace SecureFolderFS.Backend.Messages
{
    public sealed class NavigationRequestedMessage : ValueChangedMessage<BasePageViewModel?>
    {
        public TransitionModel? Transition { get; init; }

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
