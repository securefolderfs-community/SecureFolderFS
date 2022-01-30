using CommunityToolkit.Mvvm.Messaging.Messages;
using SecureFolderFS.Backend.Models;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class LockVaultRequestedMessage : ValueChangedMessage<VaultModel>
    {
        public LockVaultRequestedMessage(VaultModel value)
            : base(value)
        {
        }
    }
}
