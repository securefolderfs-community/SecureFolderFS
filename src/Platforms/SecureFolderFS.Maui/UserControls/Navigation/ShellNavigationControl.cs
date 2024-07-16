using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Maui.Views.Vault;
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
                VaultOverviewViewModel => "OverviewPage",
                VaultPropertiesViewModel => "PropertiesPage",
                _ => throw new ArgumentOutOfRangeException(nameof(target))
            };

            // Logging in, remove LoginPage when going back
            if (target is VaultOverviewViewModel && Shell.Current.CurrentPage is LoginPage)
                url = $"../{url}";

            // Logging out, remove OverviewPage when going back
            if (target is VaultLoginViewModel && Shell.Current.CurrentPage is OverviewPage)
                url = $"../{url}";

            await Shell.Current.GoToAsync(url, target.ToViewModelParameter());
            return true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
