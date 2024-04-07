using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.Maui.UserControls.Navigation
{
    /// <inheritdoc cref="INavigationControl"/>
    internal sealed class ShellNavigationControl : INavigationControl
    {
        public static ShellNavigationControl Instance { get; } = new();

        private ShellNavigationControl()
        {
        }

        /// <inheritdoc/>
        public async Task<bool> NavigateAsync<TTarget, TTransition>(TTarget? target, TTransition? transition = default)
            where TTransition : class
            where TTarget : IViewDesignation
        {
            var url = target switch
            {
                VaultLoginPageViewModel => "LoginPage",
                VaultDashboardPageViewModel => "DashboardPage",
                _ => throw new ArgumentOutOfRangeException(nameof(target))
            };

            await Shell.Current.GoToAsync(url, target.ToViewModelParameter());
            return true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
