using SecureFolderFS.Sdk.Enums;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a target which can be navigated to.
    /// </summary>
    public interface INavigationTarget : INotifyPropertyChanged
    {
        /// <summary>
        /// Notifies the implementation that the target has been navigated to.
        /// </summary>
        /// <param name="navigationType">Informs how the navigation was triggered.</param>
        void OnNavigatingTo(NavigationType navigationType);

        /// <summary>
        /// Notifies the implementation that the target has been navigated from.
        /// </summary>
        void OnNavigatingFrom();
    }
}
