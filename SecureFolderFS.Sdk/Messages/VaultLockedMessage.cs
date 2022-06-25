using SecureFolderFS.Sdk.ViewModels;

namespace SecureFolderFS.Sdk.Messages
{
    public sealed class VaultLockedMessage : ValueMessage<VaultViewModelDeprecated>
    {
        public VaultLockedMessage(VaultViewModelDeprecated value)
            : base(value)
        {
        }
    }
}
