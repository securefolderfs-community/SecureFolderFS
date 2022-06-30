using SecureFolderFS.Sdk.ViewModels;

namespace SecureFolderFS.Sdk.Messages
{
    // TODO: Maybe deprecate it?
    public sealed class VaultUnlockedMessage : ValueMessage<VaultViewModelDeprecated>
    {
        public VaultUnlockedMessage(VaultViewModelDeprecated value)
            : base(value)
        {
        }
    }
}
