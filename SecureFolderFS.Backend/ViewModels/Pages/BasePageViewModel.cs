using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.Utils;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Pages
{
    public abstract class BasePageViewModel : ObservableObject, ICleanable, IDisposable
    {
        public IMessenger Messenger { get; }

        protected VaultModel VaultModel { get; }

        protected BasePageViewModel(IMessenger messenger, VaultModel vaultModel)
        {
            this.Messenger = messenger;
            this.VaultModel = vaultModel;
        }

        public virtual void Cleanup() { }

        public virtual void Dispose() { }
    }
}
