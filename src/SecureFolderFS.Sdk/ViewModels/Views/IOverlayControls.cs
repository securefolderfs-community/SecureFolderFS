using SecureFolderFS.Shared.ComponentModel;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views
{
    /// <summary>
    /// Represents basic overlay controls to display in the UI.
    /// </summary>
    public interface IOverlayControls : IViewable, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets whether the continuation should be possible or not.
        /// </summary>
        public bool CanContinue { get; }

        /// <summary>
        /// Gets whether the cancellation should be possible or not.
        /// </summary>
        public bool CanCancel { get; }

        /// <summary>
        /// Gets the text of primary action. If value is null, the action is hidden.
        /// </summary>
        public string? PrimaryText { get; }

        /// <summary>
        /// Gets the text of secondary action. If value is null, the action is hidden.
        /// </summary>
        public string? SecondaryText { get; }
    }
}
