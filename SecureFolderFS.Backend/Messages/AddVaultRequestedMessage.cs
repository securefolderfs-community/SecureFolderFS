using CommunityToolkit.Mvvm.Messaging.Messages;
using SecureFolderFS.Backend.Models;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class AddVaultRequestedMessage : ValueChangedMessage<VaultModel>
    {
        public AddVaultRequestedMessage(VaultModel value)
            : base(value)
        {
        }
    }
}
