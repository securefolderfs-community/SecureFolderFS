using System;
using System.Collections.Generic;
using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.AvaloniaUI.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault;
using SecureFolderFS.Sdk.ViewModels.Vault;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <inheritdoc cref="NavigationControl"/>
    internal sealed class VaultNavigationControl : NavigationControl
    {
        public Dictionary<VaultViewModel, BaseVaultPageViewModel> NavigationCache { get; }

        public VaultNavigationControl()
        {
            NavigationCache = new();
        }

        public override void Navigate<TViewModel>(TViewModel viewModel, NavigationTransition? transition)
        {
            if (viewModel is not BaseVaultPageViewModel pageViewModel)
                throw new ArgumentException($"{nameof(viewModel)} does not inherit from {nameof(BaseVaultPageViewModel)}.");

            if (pageViewModel is VaultLoginPageViewModel && (NavigationCache.TryGetValue(pageViewModel.VaultViewModel, out var existing)) && existing is VaultDashboardPageViewModel)
                transition ??= new PopUpNavigationTransition();

            // Standard animation
            transition ??= new EntranceNavigationTransition();

            // Set or update the view model for individual page
            NavigationCache[pageViewModel.VaultViewModel] = pageViewModel;

            var pageType = viewModel switch
            {
                VaultLoginPageViewModel => typeof(VaultLoginPage),
                VaultDashboardPageViewModel => typeof(VaultDashboardPage),
                _ => throw new ArgumentNullException(nameof(viewModel))
            };

             Navigate(pageType, viewModel, transition);
        }

        public void ClearContent()
        {
            CurrentContent = null;
        }
    }
}