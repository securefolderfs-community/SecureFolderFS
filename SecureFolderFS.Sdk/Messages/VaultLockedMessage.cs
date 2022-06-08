using SecureFolderFS.Sdk.ViewModels;

namespace SecureFolderFS.Sdk.Messages
{
    public sealed class VaultLockedMessage : ValueMessage<VaultViewModel>
    {
        public VaultLockedMessage(VaultViewModel value)
            : base(value)
        {
        }
    }
}
