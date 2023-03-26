using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard
{
    public abstract class BaseDashboardPageViewModel : ObservableObject, INavigationTarget, IAsyncInitialize, IDisposable
    {
        protected UnlockedVaultViewModel UnlockedVaultViewModel { get; }

        protected IStateNavigationModel DashboardNavigationModel { get; }

        protected BaseDashboardPageViewModel(UnlockedVaultViewModel unlockedVaultViewModel, IStateNavigationModel dashboardNavigationModel)
        {
            UnlockedVaultViewModel = unlockedVaultViewModel;
            DashboardNavigationModel = dashboardNavigationModel;
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
