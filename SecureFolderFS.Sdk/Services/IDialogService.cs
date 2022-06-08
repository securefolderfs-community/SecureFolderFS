using System.ComponentModel;
using SecureFolderFS.Sdk.Dialogs;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Sdk.Services
{
    public interface IDialogService
    {
        IDialog<TViewModel> GetDialog<TViewModel>(TViewModel viewModel) where TViewModel : INotifyPropertyChanged;

        Task<DialogResult> ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : INotifyPropertyChanged;
    }
}
