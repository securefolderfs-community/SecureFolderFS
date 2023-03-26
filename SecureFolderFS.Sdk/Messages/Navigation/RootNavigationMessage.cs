using System.ComponentModel;
using SecureFolderFS.Sdk.Messages.Navigation;

namespace SecureFolderFS.Sdk.Messages
{
    /// <inheritdoc cref="NavigationMessage"/>
    public sealed class RootNavigationMessage : NavigationMessage
    {
        public RootNavigationMessage(INotifyPropertyChanged viewModel)
            : base(viewModel)
        {
        }
    }
}
