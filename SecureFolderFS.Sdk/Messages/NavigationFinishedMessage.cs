using SecureFolderFS.Sdk.ViewModels.Pages;

namespace SecureFolderFS.Sdk.Messages
{
    public sealed class NavigationFinishedMessage : ValueMessage<BasePageViewModel>
    {
        public NavigationFinishedMessage(BasePageViewModel value)
            : base(value)
        {
        }
    }
}
