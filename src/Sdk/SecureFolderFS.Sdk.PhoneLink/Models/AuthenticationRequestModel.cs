using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.PhoneLink.Models
{
    [Bindable(true)]
    public sealed partial class AuthenticationRequestModel : ObservableObject
    {
        [ObservableProperty] private string _VaultName;
        [ObservableProperty] private string _DesktopName;
        [ObservableProperty] private string _DesktopType;
        [ObservableProperty] private string _CredentialName;

        public AuthenticationRequestModel(string vaultName, string desktopName, string desktopType, string credentialName)
        {
            VaultName = vaultName;
            DesktopName = desktopName;
            DesktopType = desktopType;
            CredentialName = credentialName;
        }
    }
}
