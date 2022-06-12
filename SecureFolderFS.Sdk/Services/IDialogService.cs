using System.ComponentModel;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Dialogs;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Sdk.Services
{
    public interface IDialogService
    {
        IDialog<TViewModel> GetDialog<TViewModel>(TViewModel viewModel) where TViewModel : INotifyPropertyChanged;

        Task<DialogResult> ShowDialogAsync<TViewModel>(TViewModel viewModel) where TViewModel : INotifyPropertyChanged;
    }
}
