using CommunityToolkit.Mvvm.Messaging.Messages;
using SecureFolderFS.Backend.Models;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class RemoveVaultRequestedMessage : ValueChangedMessage<VaultIdModel>
    {
        public RemoveVaultRequestedMessage(VaultIdModel value)
            : base(value)
        {
        }
    }
}
