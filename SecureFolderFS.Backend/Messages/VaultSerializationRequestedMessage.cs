using CommunityToolkit.Mvvm.Messaging.Messages;
using SecureFolderFS.Backend.ViewModels;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class VaultSerializationRequestedMessage : ValueChangedMessage<VaultViewModel>
    {
        public VaultSerializationRequestedMessage(VaultViewModel value)
            : base(value)
        {
        }
    }
}
