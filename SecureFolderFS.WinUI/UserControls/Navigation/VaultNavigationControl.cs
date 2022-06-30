using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.ViewModels.Pages;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault;
using SecureFolderFS.WinUI.Views.Vault;

namespace SecureFolderFS.WinUI.UserControls.Navigation
{
    /// <inheritdoc cref="NavigationControl"/>
    internal sealed class VaultNavigationControl : NavigationControl
    {
        public List<BaseVaultPageViewModel> NavigationCache { get; }

        public VaultNavigationControl()
        {
            NavigationCache = new();
        }

        /// <inheritdoc/>
        public override void Navigate<TViewModel>(TViewModel viewModel, NavigationTransitionInfo? transitionInfo)
        {
            if (viewModel is BaseVaultPageViewModel pageViewModel && NavigationCache.FirstOrDefault(x => pageViewModel.Equals(x)) is null)
            {
                // TODO: This is just a test code
                NavigationCache.Add(pageViewModel);
            }

            var pageType = viewModel switch
            {
                VaultLoginPageViewModel => typeof(VaultLoginPage),
                VaultDashboardPageViewModel => typeof(VaultDashboardPage),
                _ => throw new ArgumentNullException(nameof(viewModel))
            };

            ContentFrame.Navigate(pageType, viewModel, transitionInfo);
        }
    }
}
