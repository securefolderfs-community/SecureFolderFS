using System;
using SecureFolderFS.Sdk.ViewModels;

namespace SecureFolderFS.Sdk.Messages
{
    [Obsolete("This class has been deprecated.")]
    public sealed class VaultSerializationRequestedMessage : ValueMessage<VaultViewModelDeprecated>
    {
        public VaultSerializationRequestedMessage(VaultViewModelDeprecated value)
            : base(value)
        {
        }
    }
}
