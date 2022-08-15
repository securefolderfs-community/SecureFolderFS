using System.ComponentModel;

namespace SecureFolderFS.Sdk.Messages.Navigation
{
    /// <summary>
    /// Represents a request message used to navigate pages.
    /// </summary>
    public class NavigationRequestedMessage : IMessage
    {
        /// <summary>
        /// Gets the view model associated with the navigation.
        /// </summary>
        public INotifyPropertyChanged ViewModel { get; }

        public NavigationRequestedMessage(INotifyPropertyChanged viewModel)
        {
            ViewModel = viewModel;
        }
    }
}
