using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.Utils
{
    /// <inheritdoc cref="INavigationService"/>
    public interface INavigationControlContract
    {
        /// <summary>
        /// Sets the control used for navigation.
        /// </summary>
        public INavigator? Navigator { get; set; }
    }
}
