using SecureFolderFS.Backend.ViewModels;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class VaultLockedMessage : ValueMessage<VaultViewModel>
    {
        public VaultLockedMessage(VaultViewModel value)
            : base(value)
        {
        }
    }
}
