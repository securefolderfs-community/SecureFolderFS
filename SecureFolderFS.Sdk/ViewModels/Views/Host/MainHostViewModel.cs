using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Controls.Sidebar;
using SecureFolderFS.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Host
{
    public sealed class MainHostViewModel : ObservableObject, IAsyncInitialize
    {
        public INavigationModel NavigationModel { get; }

        public SidebarViewModel SidebarViewModel { get; }

        public MainHostViewModel(IVaultCollectionModel vaultCollectionModel)
        {
            NavigationModel = null!;
            SidebarViewModel = new(vaultCollectionModel);
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await SidebarViewModel.InitAsync(cancellationToken);
        }
    }
}
