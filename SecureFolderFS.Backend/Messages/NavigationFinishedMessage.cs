using CommunityToolkit.Mvvm.Messaging.Messages;
using SecureFolderFS.Backend.ViewModels.Pages;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class NavigationFinishedMessage : ValueChangedMessage<BasePageViewModel>
    {
        public NavigationFinishedMessage(BasePageViewModel value)
            : base(value)
        {
        }
    }
}
