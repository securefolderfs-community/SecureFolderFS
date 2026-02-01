using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.PhoneLink.ViewModels
{
    [Bindable(true)]
    public sealed partial class PairingRequestViewModel : ObservableObject
    {
        [ObservableProperty] private string _DesktopName;
        [ObservableProperty] private string _CredentialId;
        [ObservableProperty] private string _VerificationCode;

        public PairingRequestViewModel(string desktopName, string credentialId, string verificationCode)
        {
            DesktopName = desktopName;
            CredentialId = credentialId;
            VerificationCode = verificationCode;
        }
    }
}