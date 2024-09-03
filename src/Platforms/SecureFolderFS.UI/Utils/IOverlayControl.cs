using SecureFolderFS.Shared.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.Utils
{
    /// <summary>
    /// Represents an overlay with an associated view model.
    /// </summary>
    public interface IOverlayControl
    {
        /// <summary>
        /// Shows the overlay and waits until an option is selected or the overlay is closed.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult"/> of the operation.</returns>
        /// <remarks>
        /// The return value may contain additional information about the chosen option.
        /// </remarks>
        Task<IResult> ShowAsync();

        /// <summary>
        /// Sets the view associated with the overlay.
        /// </summary>
        /// <param name="viewable">The backing <see cref="IViewable"/> view to set.</param>
        void SetView(IViewable viewable);

        /// <summary>
        /// Hides the overlay if possible.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task HideAsync();
    }
}
