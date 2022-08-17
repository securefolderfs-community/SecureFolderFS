using System.ComponentModel;
using SecureFolderFS.Sdk.Messages.Navigation;

namespace SecureFolderFS.Sdk.Messages
{
    /// <inheritdoc cref="NavigationRequestedMessage"/>
    public sealed class RootNavigationRequestedMessage : NavigationRequestedMessage
    {
        public RootNavigationRequestedMessage(INotifyPropertyChanged viewModel)
            : base(viewModel)
        {
        }
    }
}
