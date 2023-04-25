using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Sidebar;
using SecureFolderFS.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Host
{
    public sealed class MainHostViewModel : ObservableObject, IAsyncInitialize
    {
        public INavigationService NavigationService { get; } = Ioc.Default.GetRequiredService<INavigationService>();

        public SidebarViewModel SidebarViewModel { get; }

        public MainHostViewModel(IVaultCollectionModel vaultCollectionModel)
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
