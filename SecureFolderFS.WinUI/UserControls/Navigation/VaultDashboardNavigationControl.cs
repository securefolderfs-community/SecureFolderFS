using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using SecureFolderFS.WinUI.Views.Vault;
using System;
using System.Collections.Generic;

namespace SecureFolderFS.WinUI.UserControls.Navigation
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
        protected override bool NavigateFrame(Type pageType, object parameter, NavigationTransitionInfo? transitionInfo)
        {
            transitionInfo ??= new SlideNavigationTransitionInfo();
            return ContentFrame.Navigate(pageType, pageType, transitionInfo);
        }
    }
}
