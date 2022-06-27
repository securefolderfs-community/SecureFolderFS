using System.ComponentModel;

namespace SecureFolderFS.Sdk.Messages.Navigation
{
    /// <summary>
    /// Represents a request message used to navigate pages.
    /// </summary>
    /// <typeparam name="TViewModel">The type of view model.</typeparam>
    public class NavigationRequestedMessage<TViewModel> : IMessage
        where TViewModel : class, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the view model associated with the navigation.
        /// </summary>
        public TViewModel ViewModel { get; }

        public NavigationRequestedMessage(TViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }

    /// <inheritdoc cref="NavigationRequestedMessage{TViewModel}"/>
    public class NavigationRequestedMessage : NavigationRequestedMessage<INotifyPropertyChanged>
    {
        public NavigationRequestedMessage(INotifyPropertyChanged viewModel)
            : base(viewModel)
        {
        }
    }
}
