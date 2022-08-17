using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Sidebar;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.AppHost
{
    public sealed class MainAppHostViewModel : ObservableObject, IAsyncInitialize
    {
        public SidebarViewModel SidebarViewModel { get; }

        public MainAppHostViewModel(IVaultCollectionModel vaultCollectionModel)
        {
            SidebarViewModel = new(vaultCollectionModel);
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await SidebarViewModel.InitAsync(cancellationToken);
        }
    }
}
