using SecureFolderFS.Shared.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service to manage dialogs.
    /// </summary>
    public interface IOverlayService
    {
        /// <summary>
        /// Gets the currently shown overlay.
        /// </summary>
        IViewable? CurrentView { get; }

        /// <summary>
        /// Shows the provided <paramref name="viewable"/> as an overlay.
        /// If an overlay is already shown, a new one will be presented on top of the existing one.
        /// </summary>
        /// <param name="viewable">The view to present.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult"/> of the operation.</returns>
        /// <remarks>
        /// The return value may contain additional information about the chosen option.
        /// </remarks>
        Task<IResult> ShowAsync(IViewable viewable);

        /// <summary>
        /// Closes all currently displayed overlays.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task CloseAllAsync();
    }
}
