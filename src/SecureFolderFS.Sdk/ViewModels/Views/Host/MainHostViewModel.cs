using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.VaultList;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Host
{
    [Inject<INavigationService>(Visibility = "public")]
    public sealed partial class MainHostViewModel : BasePageViewModel
    {
        private readonly INavigationService _hostNavigationService;

        public VaultListViewModel VaultListViewModel { get; }

        public MainHostViewModel(INavigationService hostNavigationService, IVaultCollectionModel vaultCollectionModel)
        {
            ServiceProvider = Ioc.Default;
            _hostNavigationService = hostNavigationService;
            VaultListViewModel = new(vaultCollectionModel);
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            return VaultListViewModel.InitAsync(cancellationToken);
        }
    }
}
