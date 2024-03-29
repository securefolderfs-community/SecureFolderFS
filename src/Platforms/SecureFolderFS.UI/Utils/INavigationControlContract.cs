using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.UI.Utils
{
    /// <inheritdoc cref="INavigationService"/>
    public interface INavigationControlContract
    {
        /// <summary>
        /// Sets the control used for navigation.
        /// </summary>
        public INavigationControl? NavigationControl { set; }
    }
}
