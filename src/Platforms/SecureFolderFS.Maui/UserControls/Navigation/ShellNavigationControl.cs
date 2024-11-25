using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Maui.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Browser;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.Maui.UserControls.Navigation
{
    /// <inheritdoc cref="INavigationControl"/>
    internal sealed class ShellNavigationControl : INavigationControl
    {
        public static ShellNavigationControl Instance { get; } = new();

        /// <inheritdoc/>
        public async Task<bool> NavigateAsync<TTarget, TTransition>(TTarget? target, TTransition? transition = default)
            where TTransition : class
            where TTarget : IViewDesignation
        {
            var url = target switch
            {
                VaultLoginViewModel => "LoginPage",
                VaultDashboardViewModel => "OverviewPage",
                BrowserViewModel => "BrowserPage",
                _ => throw new ArgumentOutOfRangeException(nameof(target))
            };

            url = ParseUrl(target, url);
            await Shell.Current.GoToAsync(url, target.ToViewModelParameter());
            
            return true;
        }

        private static string ParseUrl<TTarget>(TTarget? target, string url)
        {
            // Logging in, remove LoginPage when going back
            if (target is VaultDashboardViewModel && Shell.Current.CurrentPage is LoginPage)
                url = $"../{url}";

            // Logging out, remove OverviewPage when going back
            if (target is VaultLoginViewModel && Shell.Current.CurrentPage is OverviewPage)
                url = $"../{url}";

            return url;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
