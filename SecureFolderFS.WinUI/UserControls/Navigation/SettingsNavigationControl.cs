using System;
using SecureFolderFS.Sdk.Models.Transitions;
using SecureFolderFS.Sdk.ViewModels.Pages.SettingsPages;
using SecureFolderFS.WinUI.Helpers;
using SecureFolderFS.WinUI.Views.Settings;

namespace SecureFolderFS.WinUI.UserControls.Navigation
{
    public sealed class SettingsNavigationControl : NavigationControl
    {
        public override void Navigate<TViewModel>(TViewModel viewModel, TransitionModel? transition)
        {
            var pageType = viewModel switch
            {
                GeneralSettingsPageViewModel => typeof(GeneralSettingsPage),
                PreferencesSettingsPageViewModel => typeof(PreferencesSettingsPage),
                PrivacySettingsPageViewModel => typeof(PreferencesSettingsPage),
                AboutSettingsPageViewModel => typeof(AboutSettingsPage),
                _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
            };

            var transitionInfo = ConversionHelpers.ToNavigationTransitionInfo(transition);
            ContentFrame.Navigate(pageType, viewModel, transitionInfo);
        }
    }
}
