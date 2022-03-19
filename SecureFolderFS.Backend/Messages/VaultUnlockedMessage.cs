using SecureFolderFS.Backend.ViewModels;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class VaultUnlockedMessage : ValueMessage<VaultViewModel>
    {
        public VaultUnlockedMessage(VaultViewModel value)
            : base(value)
        {
        }
    }
}
