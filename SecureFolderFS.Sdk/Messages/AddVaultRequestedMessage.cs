using SecureFolderFS.Sdk.ViewModels;

namespace SecureFolderFS.Sdk.Messages
{
    public sealed class AddVaultRequestedMessage : ValueMessage<VaultViewModel>
    {
        public AddVaultRequestedMessage(VaultViewModel value)
            : base(value)
        {
        }
    }
}
