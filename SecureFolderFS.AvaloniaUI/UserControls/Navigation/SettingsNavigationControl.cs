using System;
using FluentAvalonia.UI.Media.Animation;
using SecureFolderFS.AvaloniaUI.Views.Settings;
using SecureFolderFS.Sdk.ViewModels.Pages.Settings;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
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
                PrivacySettingsPageViewModel => typeof(PrivacySettingsPage),
                AboutSettingsPageViewModel => typeof(AboutSettingsPage),
                _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
            };

            ContentFrame.Navigate(pageType, viewModel, transitionInfo);
        }
    }
}