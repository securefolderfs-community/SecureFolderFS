using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.PhoneLink.ViewModels
{
    [Bindable(true)]
    public sealed partial class PairingRequestViewModel : ObservableObject
    {
        [ObservableProperty] private string _DesktopName;
        [ObservableProperty] private string _DesktopType;
        [ObservableProperty] private string _CredentialId;
        [ObservableProperty] private string _VerificationCode;

        public PairingRequestViewModel(string desktopName, string desktopType, string credentialId, string verificationCode)
        {
            DesktopName = desktopName;
            DesktopType = desktopType;
            CredentialId = credentialId;
            VerificationCode = verificationCode;
        }
    }
}