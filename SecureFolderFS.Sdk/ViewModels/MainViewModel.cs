using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Pages;
using SecureFolderFS.Sdk.ViewModels.Sidebar;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels
{
    public sealed class MainViewModel : ObservableObject, IAsyncInitialize
    {
        private IVaultCollectionModel VaultCollection { get; }

        public SidebarViewModel SidebarViewModel { get; }

        public BasePageViewModel? CurrentPageViewModel { get; set; }

        public MainViewModel()
        {
            VaultCollection = new LocalVaultCollectionModel();
            SidebarViewModel = new(VaultCollection);
        }

        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return SidebarViewModel.InitAsync(cancellationToken);
        }
    }
}
