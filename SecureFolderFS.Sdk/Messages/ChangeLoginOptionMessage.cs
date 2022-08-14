using SecureFolderFS.Sdk.ViewModels.Vault.LoginStrategy;

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
        public BaseLoginStrategyViewModel ViewModel { get; }

        public ChangeLoginOptionMessage(BaseLoginStrategyViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }
}
