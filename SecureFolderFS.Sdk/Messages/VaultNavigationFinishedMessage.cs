using SecureFolderFS.Sdk.ViewModels.Pages;

namespace SecureFolderFS.Sdk.Messages
{
    public sealed class VaultNavigationFinishedMessage : ValueMessage<BasePageViewModel>
    {
        public VaultNavigationFinishedMessage(BasePageViewModel value)
            : base(value)
        {
        }
    }
}
