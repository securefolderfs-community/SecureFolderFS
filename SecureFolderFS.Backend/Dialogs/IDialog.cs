using System.ComponentModel;
using SecureFolderFS.Backend.Enums;

namespace SecureFolderFS.Backend.Dialogs
{
    public interface IDialog<TViewModel>
        where TViewModel : INotifyPropertyChanged
    {
        TViewModel ViewModel { get; set; }

        Task<DialogResult> ShowAsync();

        void Hide();
    }
}
