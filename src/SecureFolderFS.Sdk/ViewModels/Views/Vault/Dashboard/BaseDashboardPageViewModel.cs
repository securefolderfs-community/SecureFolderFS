using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard
{
    public abstract partial class BaseDashboardPageViewModel : ObservableObject, IViewDesignation, IAsyncInitialize
    {
        [ObservableProperty] private string? _Title;

        protected UnlockedVaultViewModel UnlockedVaultViewModel { get; }

        protected INavigationService DashboardNavigationService { get; }

        protected BaseDashboardPageViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigationService dashboardNavigationService)
        {
            UnlockedVaultViewModel = unlockedVaultViewModel;
            DashboardNavigationService = dashboardNavigationService;
        }

        /// <inheritdoc/>
        public virtual void OnAppearing()
        {
        }

        /// <inheritdoc/>
        public virtual void OnDisappearing()
        {
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);
    }
}
