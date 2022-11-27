using SecureFolderFS.Sdk.Enums;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    /// <typeparam name="TViewModel">The view model type related to dialog.</typeparam>
    /// <inheritdoc cref="IDialog"/>
    public interface IDialog<TViewModel> : IDialog
        where TViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the view model associated with the dialog.
        /// </summary>
        TViewModel ViewModel { get; set; }
    }

    /// <summary>
    /// Represents a dialog with an associated view model.
    /// </summary>
    public interface IDialog
    {
        /// <summary>
        /// Shows the dialog and awaits until an option is selected or the dialog is closed.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns <see cref="DialogResult"/> based on the selected option.</returns>
        Task<DialogResult> ShowAsync();

        /// <summary>
        /// Hides the dialog if possible.
        /// </summary>
        void Hide();
    }
}
