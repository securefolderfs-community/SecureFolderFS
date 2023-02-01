using System;
using SecureFolderFS.AvaloniaUI.Animations.Transitions;
using SecureFolderFS.AvaloniaUI.Views.Settings;
using SecureFolderFS.Sdk.ViewModels.Pages.Settings;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <inheritdoc cref="NavigationControl"/>
    internal sealed class SettingsNavigationControl : NavigationControl
    {
        /// <inheritdoc/>
        public override void Navigate<TViewModel>(TViewModel viewModel, TransitionBase? transition)
        {
            var pageType = viewModel switch
            {
                GeneralSettingsPageViewModel => typeof(GeneralSettingsPage),
                PreferencesSettingsPageViewModel => typeof(PreferencesSettingsPage),
                PrivacySettingsPageViewModel => typeof(PrivacySettingsPage),
                AboutSettingsPageViewModel => typeof(AboutSettingsPage),
                _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
            };

            Navigate(pageType, viewModel, transition);
        }
    }
}