using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Utils;

namespace SecureFolderFS.Backend.ViewModels.Pages
{
    public abstract class BasePageViewModel : ObservableObject, ICleanable, IDisposable
    {
        public IMessenger Messenger { get; }

        protected VaultViewModel VaultViewModel { get; }

        protected BasePageViewModel(IMessenger messenger, VaultViewModel vaultViewModel)
        {
            this.Messenger = messenger;
            this.VaultViewModel = vaultViewModel;
        }

        public virtual void Cleanup() { }

        public virtual void Dispose() { }
    }
}
