using SecureFolderFS.AvaloniaUI.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using System;
using System.Collections.Generic;
using FluentAvalonia.UI.Media.Animation;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <inheritdoc cref="FrameNavigationControl"/>
    internal sealed class VaultDashboardNavigationControl : FrameNavigationControl
    {
        /// <inheritdoc/>
        public override Dictionary<Type, Type> TypeBinding { get; } = new()
        {
            { typeof(VaultOverviewPageViewModel), typeof(VaultOverviewPage) },
            { typeof(VaultPropertiesPageViewModel), typeof(VaultPropertiesPage) }
        };

        /// <inheritdoc/>
        protected override bool NavigateFrame(Type pageType, object parameter, NavigationTransitionInfo? transition)
        {
            transition ??= new EntranceNavigationTransitionInfo();
            return ContentFrame.Navigate(pageType, parameter, transition);
        }
    }
}