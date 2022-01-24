using CommunityToolkit.Mvvm.Messaging.Messages;
using SecureFolderFS.Backend.ViewModels.Pages;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class NavigationRequestedMessage : ValueChangedMessage<BasePageViewModel>
    {
        public NavigationRequestedMessage(BasePageViewModel value)
            : base(value)
        {
        }
    }
}
