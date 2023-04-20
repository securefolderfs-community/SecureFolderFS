using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Sdk.ViewModels.Views
{
    /// <summary>
    /// Represents a target which can be navigated to.
    /// </summary>
    public interface INavigationTarget
    {
        /// <summary>
        /// Notifies the implementation that the target is being navigated to.
        /// </summary>
        /// <param name="navigationType">Informs how the navigation was triggered.</param>
        void OnNavigatingTo(NavigationType navigationType);

        /// <summary>
        /// Notifies the implementation that the target is being navigated from.
        /// </summary>
        void OnNavigatingFrom();
    }
}
