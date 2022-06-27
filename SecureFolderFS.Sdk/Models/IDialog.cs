using System.ComponentModel;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a dialog.
    /// </summary>
    /// <typeparam name="TViewModel">The view model type related to dialog.</typeparam>
    public interface IDialog<TViewModel>
        where TViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the view model associated with the dialog.
        /// </summary>
        TViewModel ViewModel { get; set; }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns <see cref="DialogResult"/> based on the selected option.</returns>
        Task<DialogResult> ShowAsync();

        /// <summary>
        /// Hides the dialog if it is open.
        /// </summary>
        void Hide();
    }
}
