using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Pickers;

namespace SecureFolderFS.Sdk.ViewModels.Accounts
{
    [Bindable(true)]
    public abstract class AccountViewModel : ObservableObject
    {
        public IFilePicker? FilePicker { get; protected set; }

        public abstract Task<IResult> ConnectAsync();

        public abstract Task DisconnectAsync();
    }
}