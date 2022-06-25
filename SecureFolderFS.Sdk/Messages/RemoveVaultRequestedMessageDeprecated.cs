using System;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Messages
{
    [Obsolete("Class replaced by RemoveVaultMessage.")]
    public sealed class RemoveVaultRequestedMessageDeprecated : ValueMessage<VaultIdModel>
    {
        public RemoveVaultRequestedMessageDeprecated(VaultIdModel value)
            : base(value)
        {
        }
    }
}
