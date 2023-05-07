using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using System.ComponentModel;
using System.Threading.Tasks;

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
        /// Creates and shows appropriate dialog derived from associated <paramref name="viewModel"/>.
        /// </summary>
        /// <typeparam name="TViewModel">The type of view model.</typeparam>
        /// <param name="viewModel">The view model of the dialog.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns <see cref="DialogResult"/> based on the selected option.</returns>
        Task<DialogResult> ShowDialogAsync<TViewModel>(TViewModel viewModel) where TViewModel : class, INotifyPropertyChanged;
    }
}
