using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.WinUI.Views.Vault;
using System;
using System.Collections.Generic;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.WinUI.UserControls.Navigation
{
    /// <inheritdoc cref="FrameNavigationControl"/>
    internal sealed class VaultNavigationControl : FrameNavigationControl
    {
        /// <inheritdoc/>
        public override Dictionary<Type, Type> TypeBinding { get; } = new()
        {
            { typeof(VaultLoginPageViewModel), typeof(VaultLoginPage) },
            { typeof(VaultDashboardPageViewModel), typeof(VaultDashboardPage) }
        };

        /// <inheritdoc/>
        protected override bool NavigateFrame(Type pageType, object parameter, NavigationTransitionInfo? transitionInfo)
        {
            transitionInfo ??= parameter switch
            {
                VaultLoginPageViewModel => new ContinuumNavigationTransitionInfo(), // Dashboard closing animation
                _ => new EntranceNavigationTransitionInfo() // Standard animation
            };

            return ContentFrame.Navigate(pageType, parameter, transitionInfo);
        }

        public void ClearContent()
        {
            ContentFrame.Content = null;
        }
    }
}
