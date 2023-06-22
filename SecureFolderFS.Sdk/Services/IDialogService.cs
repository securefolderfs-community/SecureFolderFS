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
    }
}
