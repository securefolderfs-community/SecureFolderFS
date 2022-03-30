using SecureFolderFS.Backend.ViewModels.Pages;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class NavigationFinishedMessage : ValueMessage<BasePageViewModel>
    {
        public NavigationFinishedMessage(BasePageViewModel value)
            : base(value)
        {
        }
    }
}
