using SecureFolderFS.Sdk.ViewModels;

namespace SecureFolderFS.Sdk.Messages
{
    public sealed class VaultSerializationRequestedMessage : ValueMessage<VaultViewModel>
    {
        public VaultSerializationRequestedMessage(VaultViewModel value)
            : base(value)
        {
        }
    }
}
