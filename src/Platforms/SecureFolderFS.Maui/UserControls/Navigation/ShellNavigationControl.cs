using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Maui.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Maui.UserControls.Navigation
{
    /// <inheritdoc cref="INavigator"/>
    internal sealed class ShellNavigationControl : INavigator
    {
        public static ShellNavigationControl Instance { get; } = new();

        /// <inheritdoc/>
        public async Task<bool> NavigateAsync(IViewDesignation? view)
        {
            var url = view switch
            {
                VaultLoginViewModel => "LoginPage",
                VaultDashboardViewModel => "OverviewPage",
                BrowserViewModel => "BrowserPage",
                _ => throw new ArgumentOutOfRangeException(nameof(view))
            };

            url = ParseUrl(view, url);
            await Shell.Current.GoToAsync(url, view.ToViewModelParameter());
            
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> GoBackAsync()
        {
            await Shell.Current.GoBackAsync();
            return true;
        }

        /// <inheritdoc/>
        public Task<bool> GoForwardAsync()
        {
            return Task.FromResult(false);
        }

        private static string ParseUrl<TTarget>(TTarget? target, string url)
        {
            // Implementation detail: since we are navigating (and forgetting!) to OverviewPage from LoginPage (when logging-in)
            // or to LoginPage from OverviewPage (when logging-out), the navigation stack no longer contains a reference to
            // the login/dashboard view model, and so we need to synchronize the Shell navigation and modify the url in such a way
            // that the aforementioned pages are popped from Shell in specific instances. This approach does not corrupt
            // the navigation chain, but merely aligns it with our stack.
            
            // Logging in, remove LoginPage when going back
            if (target is VaultDashboardViewModel && Shell.Current.CurrentPage is LoginPage)
                url = $"../{url}";

            // Logging out, remove OverviewPage when going back
            if (target is VaultLoginViewModel && Shell.Current.CurrentPage is OverviewPage)
                url = $"../{url}";

            return url;
        }
    }
}
