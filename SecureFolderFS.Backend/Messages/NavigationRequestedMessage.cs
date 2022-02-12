using CommunityToolkit.Mvvm.Messaging.Messages;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.Models.Transitions;
using SecureFolderFS.Backend.ViewModels;
using SecureFolderFS.Backend.ViewModels.Pages;

#nullable enable

namespace SecureFolderFS.Backend.Messages
{
    public sealed class NavigationRequestedMessage : ValueChangedMessage<BasePageViewModel?>
    {
        public TransitionModel? Transition { get; init; }

        public VaultViewModel VaultViewModel { get; }

        public NavigationRequestedMessage(VaultViewModel vaultModel, BasePageViewModel? value = null)
            : base(value)
        {
            this.VaultViewModel = vaultModel;
        }
    }
}
