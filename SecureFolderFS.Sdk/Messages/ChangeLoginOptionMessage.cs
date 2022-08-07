using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.Messages
{
    /// <summary>
    /// A request message that changes the login option view model.
    /// </summary>
    public sealed class ChangeLoginOptionMessage : IMessage
    {
        /// <summary>
        /// Gets the login option view model to change the view to.
        /// </summary>
        public ObservableObject ViewModel { get; }

        public ChangeLoginOptionMessage(ObservableObject viewModel)
        {
            ViewModel = viewModel;
        }
    }
}
