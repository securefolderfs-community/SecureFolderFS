using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Messages
{
    public sealed class RemoveVaultRequestedMessage : ValueMessage<VaultIdModel>
    {
        public RemoveVaultRequestedMessage(VaultIdModel value)
            : base(value)
        {
        }
    }
}
