using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using SecureFolderFS.WinUI.Views.Settings;
using System;

namespace SecureFolderFS.WinUI.UserControls.Navigation
{
    /// <inheritdoc cref="NavigationControl"/>
    internal sealed class SettingsNavigationControl : NavigationControl
    {
        /// <inheritdoc/>
        public override void Navigate<TViewModel>(TViewModel viewModel, NavigationTransitionInfo? transitionInfo)
        {
            var pageType = viewModel switch
            {
                GeneralSettingsViewModel => typeof(GeneralSettingsPage),
                PreferencesSettingsViewModel => typeof(PreferencesSettingsPage),
                PrivacySettingsViewModel => typeof(PrivacySettingsPage),
                AboutSettingsViewModel => typeof(AboutSettingsPage),
                _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
            };

            ContentFrame.Navigate(pageType, viewModel, transitionInfo);
        }
    }
}
