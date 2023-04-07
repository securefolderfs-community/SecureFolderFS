using SecureFolderFS.AvaloniaUI.Animations.Transitions;
using SecureFolderFS.AvaloniaUI.Views.Settings;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using System;

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
                GeneralSettingsViewModel => typeof(GeneralSettingsPage),
                PreferencesSettingsViewModel => typeof(PreferencesSettingsPage),
                PrivacySettingsViewModel => typeof(PrivacySettingsPage),
                AboutSettingsViewModel => typeof(AboutSettingsPage),
                _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
            };

            Navigate(pageType, viewModel, transition);
        }
    }
}