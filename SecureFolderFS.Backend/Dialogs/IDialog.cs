using SecureFolderFS.Backend.Enums;

namespace SecureFolderFS.Backend.Dialogs
{
    public interface IDialog<TViewModel>
    {
        TViewModel ViewModel { get; set; }

        Task<DialogResult> ShowAsync();
    }
}
