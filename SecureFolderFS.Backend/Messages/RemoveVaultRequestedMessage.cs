using CommunityToolkit.Mvvm.Messaging.Messages;
using SecureFolderFS.Backend.Models;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class RemoveVaultRequestedMessage : ValueChangedMessage<VaultModel>
    {
        public RemoveVaultRequestedMessage(VaultModel value)
            : base(value)
        {
        }
    }
}
