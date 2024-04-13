using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using SecureFolderFS.Uno.Views.Vault;
using System;
using System.Collections.Generic;

namespace SecureFolderFS.Uno.UserControls.Navigation
{
    /// <inheritdoc cref="FrameNavigationControl"/>
    internal sealed partial class VaultDashboardNavigationControl : FrameNavigationControl
    {
        private bool _isFirstTime = true;

        /// <inheritdoc/>
        public override Dictionary<Type, Type> TypeBinding { get; } = new()
        {
            { typeof(VaultOverviewViewModel), typeof(VaultOverviewPage) },
            { typeof(VaultPropertiesViewModel), typeof(VaultPropertiesPage) }
        };

        /// <inheritdoc/>
        protected override bool NavigateFrame(Type pageType, object parameter, NavigationTransitionInfo? transitionInfo)
        {
            if (_isFirstTime)
            {
                _isFirstTime = false;
                transitionInfo ??= new EntranceNavigationTransitionInfo();
            }
            else
            {
                transitionInfo ??= parameter switch
                {
                    VaultPropertiesViewModel => new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight },
                    VaultOverviewViewModel => new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft },
                    _ => new EntranceNavigationTransitionInfo()
                };
            }

            return ContentFrame.Navigate(pageType, parameter, transitionInfo);
        }
    }
}
