using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.AppModels;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public sealed class CipherItemViewModel : ObservableObject
    {
        public CipherInfoModel CipherInfoModel { get; }

        public string Name { get; }

        public CipherItemViewModel(CipherInfoModel cipherInfoModel)
        {
            CipherInfoModel = cipherInfoModel;
            Name = cipherInfoModel.Name;
        }
    }
}
