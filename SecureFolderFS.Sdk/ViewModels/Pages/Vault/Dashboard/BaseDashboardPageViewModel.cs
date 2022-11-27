using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard
{
    public abstract class BaseDashboardPageViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        protected UnlockedVaultViewModel UnlockedVaultViewModel { get; }

        protected IMessenger Messenger { get; }

        protected BaseDashboardPageViewModel(UnlockedVaultViewModel unlockedVaultViewModel, IMessenger messenger)
        {
            UnlockedVaultViewModel = unlockedVaultViewModel;
            Messenger = messenger;
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public virtual void Dispose() { }
    }
}
