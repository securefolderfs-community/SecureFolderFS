using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.PhoneLink.ViewModels
{
    [Bindable(true)]
    public sealed partial class PairingRequestViewModel : ObservableObject
    {
        [ObservableProperty] private string _VaultName;
        [ObservableProperty] private string _CredentialId;
        [ObservableProperty] private string _VerificationCode;

        public PairingRequestViewModel(string vaultName, string credentialId, string verificationCode)
        {
            VaultName = vaultName;
            CredentialId = credentialId;
            VerificationCode = verificationCode;
        }
    }
}