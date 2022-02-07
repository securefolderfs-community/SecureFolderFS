using SecureFolderFS.Backend.Dialogs;
using SecureFolderFS.Backend.Enums;

namespace SecureFolderFS.Backend.Services
{
    public interface IDialogService
    {
        IDialog<TViewModel> GetDialog<TViewModel>(TViewModel viewModel);

        Task<DialogResult> ShowDialog<TViewModel>(TViewModel viewModel);
    }
}
