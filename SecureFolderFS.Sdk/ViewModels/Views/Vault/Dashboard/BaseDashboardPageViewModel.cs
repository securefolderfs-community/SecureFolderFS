using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard
{
    public abstract class BaseDashboardPageViewModel : ObservableObject, INavigationTarget, IAsyncInitialize, IDisposable
    {
        protected UnlockedVaultViewModel UnlockedVaultViewModel { get; }

        protected INavigationService DashboardNavigationService { get; }

        protected BaseDashboardPageViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigationService dashboardNavigationService)
        {
            UnlockedVaultViewModel = unlockedVaultViewModel;
            DashboardNavigationService = dashboardNavigationService;
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public virtual void OnNavigatingTo(NavigationType navigationType) { }

        /// <inheritdoc/>
        public virtual void OnNavigatingFrom() { }

        /// <inheritdoc/>
        public virtual void Dispose() { }
    }
}
