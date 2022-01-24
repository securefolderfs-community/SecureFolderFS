using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Backend.Models;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Pages
{
    public abstract class BasePageViewModel : ObservableObject, IDisposable
    {
        public VaultModel VaultModel { get; }

        public BasePageViewModel(VaultModel vaultModel)
        {
            this.VaultModel = vaultModel;
        }

        public abstract void Dispose();
    }
}
