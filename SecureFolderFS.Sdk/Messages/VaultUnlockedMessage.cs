using SecureFolderFS.Sdk.ViewModels;

namespace SecureFolderFS.Sdk.Messages
{
    public sealed class VaultUnlockedMessage : ValueMessage<VaultViewModel>
    {
        public VaultUnlockedMessage(VaultViewModel value)
            : base(value)
        {
        }
    }
}
