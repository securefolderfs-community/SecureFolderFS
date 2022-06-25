using SecureFolderFS.Sdk.ViewModels;

namespace SecureFolderFS.Sdk.Messages
{
    public sealed class VaultSerializationRequestedMessage : ValueMessage<VaultViewModelDeprecated>
    {
        public VaultSerializationRequestedMessage(VaultViewModelDeprecated value)
            : base(value)
        {
        }
    }
}
