using SecureFolderFS.Sdk.ViewModels;

namespace SecureFolderFS.Sdk.Messages
{
    public sealed class VaultUnlockedMessage : ValueMessage<VaultViewModelDeprecated>
    {
        public VaultUnlockedMessage(VaultViewModelDeprecated value)
            : base(value)
        {
        }
    }
}
