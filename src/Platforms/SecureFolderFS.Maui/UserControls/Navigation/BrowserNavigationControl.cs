using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.Maui.UserControls.Navigation
{
    /// <inheritdoc cref="INavigator"/>
    internal sealed class BrowserNavigationControl : ContentPresentation, INavigator
    {
        /// <inheritdoc/>
        public Task<bool> NavigateAsync(IViewDesignation? view)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> GoBackAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> GoForwardAsync()
        {
            throw new NotImplementedException();
        }
    }
}
