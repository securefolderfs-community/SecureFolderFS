using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Sidebar;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Host
{
    public sealed class MainHostViewModel : BasePageViewModel
    {
        private readonly INavigationService _hostNavigationService;

        public INavigationService NavigationService { get; } = Ioc.Default.GetRequiredService<INavigationService>();

        public SidebarViewModel SidebarViewModel { get; }

        public MainHostViewModel(INavigationService hostNavigationService, IVaultCollectionModel vaultCollectionModel)
        {
            _hostNavigationService = hostNavigationService;
            SidebarViewModel = new(vaultCollectionModel);
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await SidebarViewModel.InitAsync(cancellationToken);
        }
    }
}
