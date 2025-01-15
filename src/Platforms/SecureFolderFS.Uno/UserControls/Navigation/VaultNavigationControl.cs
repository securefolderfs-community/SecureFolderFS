using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Uno.Views.Vault;

namespace SecureFolderFS.Uno.UserControls.Navigation
{
    /// <inheritdoc cref="FrameNavigationControl"/>
    internal sealed partial class VaultNavigationControl : FrameNavigationControl
    {
        /// <inheritdoc/>
        public override Dictionary<Type, Type> TypeBinding { get; } = new()
        {
            { typeof(VaultLoginViewModel), typeof(VaultLoginPage) },
            { typeof(VaultDashboardViewModel), typeof(VaultDashboardPage) }
        };

        /// <inheritdoc/>
        protected override bool NavigateFrame(Type pageType, object parameter)
        {
            NavigationTransitionInfo transitionInfo = ContentFrame.Content switch
            {
                VaultDashboardPage => new ContinuumNavigationTransitionInfo(), // Dashboard closing animation
                _ => new EntranceNavigationTransitionInfo() // Standard animation
            };

            return ContentFrame.Navigate(pageType, parameter, transitionInfo);
        }
    }
}
