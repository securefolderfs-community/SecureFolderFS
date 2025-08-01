using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Accounts
{
    [Bindable(true)]
    public sealed partial class FtpAccountViewModel : AccountViewModel
    {
        [ObservableProperty] private string? _Address;
        [ObservableProperty] private string? _Username;
        [ObservableProperty] private string? _Password;

        public override Task<IResult> ConnectAsync()
        {
            throw new System.NotImplementedException();
        }

        public override Task DisconnectAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}