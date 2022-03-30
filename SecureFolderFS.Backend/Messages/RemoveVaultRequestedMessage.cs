using SecureFolderFS.Backend.Models;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class RemoveVaultRequestedMessage : ValueMessage<VaultIdModel>
    {
        public RemoveVaultRequestedMessage(VaultIdModel value)
            : base(value)
        {
        }
    }
}
