using SecureFolderFS.Backend.ViewModels;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class VaultSerializationRequestedMessage : ValueMessage<VaultViewModel>
    {
        public VaultSerializationRequestedMessage(VaultViewModel value)
            : base(value)
        {
        }
    }
}
