using System.ComponentModel;
using SecureFolderFS.Sdk.Models.Transitions;

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

        /// <summary>
        /// Gets the transition that is applied when navigating.
        /// </summary>
        public TransitionModel? Transition { get; }

        public NavigationRequestedMessage(TViewModel viewModel, TransitionModel? transition = null)
        {
            ViewModel = viewModel;
            Transition = transition;
        }
    }

    /// <inheritdoc cref="NavigationRequestedMessage{TViewModel}"/>
    public class NavigationRequestedMessage : NavigationRequestedMessage<INotifyPropertyChanged>
    {
        public NavigationRequestedMessage(INotifyPropertyChanged viewModel, TransitionModel? transition = null)
            : base(viewModel, transition)
        {
        }
    }
}
