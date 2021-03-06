using SecureFolderFS.Backend.ViewModels;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class AddVaultRequestedMessage : ValueMessage<VaultViewModel>
    {
        public AddVaultRequestedMessage(VaultViewModel value)
            : base(value)
        {
        }
    }
}
