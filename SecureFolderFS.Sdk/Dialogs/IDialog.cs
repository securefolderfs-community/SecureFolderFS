using System.ComponentModel;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Sdk.Dialogs
{
    public interface IDialog<TViewModel>
        where TViewModel : INotifyPropertyChanged
    {
        TViewModel ViewModel { get; set; }

        Task<DialogResult> ShowAsync();

        void Hide();
    }
}
