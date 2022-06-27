using System;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.ViewModels.Pages.SettingsPages;
using SecureFolderFS.WinUI.Views.Settings;

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
                GeneralSettingsPageViewModel => typeof(GeneralSettingsPage),
                PreferencesSettingsPageViewModel => typeof(PreferencesSettingsPage),
                PrivacySettingsPageViewModel => typeof(PreferencesSettingsPage),
                AboutSettingsPageViewModel => typeof(AboutSettingsPage),
                _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
            };

            ContentFrame.Navigate(pageType, viewModel, transitionInfo);
        }
    }
}
