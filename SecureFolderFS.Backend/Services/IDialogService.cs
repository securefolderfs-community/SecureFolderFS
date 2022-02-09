using System.ComponentModel;
using SecureFolderFS.Backend.Dialogs;
using SecureFolderFS.Backend.Enums;

namespace SecureFolderFS.Backend.Services
{
    public interface IDialogService
    {
        IDialog<TViewModel> GetDialog<TViewModel>(TViewModel viewModel) where TViewModel : INotifyPropertyChanged;

        Task<DialogResult> ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : INotifyPropertyChanged;
    }
}
