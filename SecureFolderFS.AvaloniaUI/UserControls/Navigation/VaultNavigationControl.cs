using SecureFolderFS.AvaloniaUI.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using System;
using System.Collections.Generic;
using FluentAvalonia.UI.Media.Animation;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
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
            transitionInfo ??= ContentFrame.Content switch
            {
                // (TODO: Add dashboard closing animation here - infer from the current content) // Dashboard closing animation
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