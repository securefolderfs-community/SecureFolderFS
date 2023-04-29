using System.ComponentModel;

namespace SecureFolderFS.Sdk.Messages.Navigation
{
    /// <summary>
    /// Represents a request message used to navigate pages.
    /// </summary>
    public class NavigationMessage : IMessage
    {
        /// <summary>
        /// Gets the view model associated with the navigation.
        /// </summary>
        public INotifyPropertyChanged ViewModel { get; }

        public NavigationMessage(INotifyPropertyChanged viewModel)
        {
            ViewModel = viewModel;
        }
    }
}
