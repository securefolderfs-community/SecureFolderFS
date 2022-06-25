using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages
{
    public abstract class BasePageViewModel : ObservableObject, ICleanable, IDisposable
    {
        public IMessenger Messenger { get; }

        protected VaultViewModelDeprecated VaultViewModel { get; }

        protected BasePageViewModel(IMessenger messenger, VaultViewModelDeprecated vaultViewModel)
        {
            Messenger = messenger;
            VaultViewModel = vaultViewModel;
        }

        public virtual void Cleanup() { }

        public virtual void Dispose() { }
    }
}
