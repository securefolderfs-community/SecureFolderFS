using System;
using SecureFolderFS.Sdk.ViewModels;

namespace SecureFolderFS.Sdk.Messages
{
    [Obsolete("Class replaced by AddVaultMessage.")]
    public sealed class AddVaultRequestedMessageDeprecated : ValueMessage<VaultViewModelDeprecated>
    {
        public AddVaultRequestedMessageDeprecated(VaultViewModelDeprecated value)
            : base(value)
        {
        }
    }
}
