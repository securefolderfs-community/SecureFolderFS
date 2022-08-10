using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public sealed class CipherItemViewModel : ObservableObject
    {
        public ICipherInfoModel CipherInfoModel { get; }

        public string Name { get; }

        public CipherItemViewModel(ICipherInfoModel cipherInfoModel)
        {
            CipherInfoModel = cipherInfoModel;
            Name = cipherInfoModel.Name;
        }
    }
}
