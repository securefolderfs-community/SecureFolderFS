using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault;
using SecureFolderFS.WinUI.Views.Vault;

namespace SecureFolderFS.WinUI.UserControls.Navigation
{
    /// <inheritdoc cref="NavigationControl"/>
    internal sealed class VaultNavigationControl : NavigationControl
    {
        public Dictionary<IVaultModel, BaseVaultPageViewModel> NavigationCache { get; }

        public VaultNavigationControl()
        {
            NavigationCache = new();
        }

        /// <inheritdoc/>
        public override void Navigate<TViewModel>(TViewModel viewModel, NavigationTransitionInfo? transitionInfo)
        {
            if (viewModel is not BaseVaultPageViewModel pageViewModel)
                throw new ArgumentException($"{nameof(viewModel)} does not inherit from {nameof(BaseVaultPageViewModel)}.");

            // Set or update the view model for individual page
            NavigationCache[pageViewModel.VaultModel] = pageViewModel;

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
