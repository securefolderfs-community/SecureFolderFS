using System.ComponentModel;

namespace SecureFolderFS.Sdk.Messages.Navigation
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
