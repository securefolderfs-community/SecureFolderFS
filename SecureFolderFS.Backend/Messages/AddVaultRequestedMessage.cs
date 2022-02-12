using CommunityToolkit.Mvvm.Messaging.Messages;
using SecureFolderFS.Backend.ViewModels;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class AddVaultRequestedMessage : ValueChangedMessage<VaultViewModel>
    {
        public AddVaultRequestedMessage(VaultViewModel value)
            : base(value)
        {
        }
    }
}
