using SecureFolderFS.Sdk.Models;
using System.ComponentModel;
using System.Threading.Tasks;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service to manage dialogs.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Gets appropriate dialog with associated <paramref name="viewModel"/>.
        /// </summary>
        /// <typeparam name="TViewModel">The type of view model.</typeparam>
        /// <param name="viewModel">The view model of the dialog.</param>
        /// <returns>A new instance of <see cref="IDialog"/> that represents the dialog.</returns>
        IDialog GetDialog<TViewModel>(TViewModel viewModel) where TViewModel : class, INotifyPropertyChanged;

        /// <summary>
        /// Closes the currently opened dialog, if any.
        /// </summary>
        /// <remarks>
        /// This method will close only blocking dialogs that prevent other dialogs from opening.
        /// </remarks>
        void ReleaseDialog();
    }
}
