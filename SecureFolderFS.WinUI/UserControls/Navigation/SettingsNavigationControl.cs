using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using SecureFolderFS.WinUI.Views.Settings;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.UserControls.Navigation
{
    /// <inheritdoc cref="FrameNavigationControl"/>
    internal sealed class SettingsNavigationControl : FrameNavigationControl
    {
        /// <inheritdoc/>
        public override Task<bool> NavigateAsync<TTarget, TTransition>(TTarget target, TTransition? transition = default) where TTransition : class
        {
            var pageType = target switch
            {
                GeneralSettingsViewModel => typeof(GeneralSettingsPage),
                PreferencesSettingsViewModel => typeof(PreferencesSettingsPage),
                PrivacySettingsViewModel => typeof(PrivacySettingsPage),
                AboutSettingsViewModel => typeof(AboutSettingsPage),
                _ => throw new ArgumentOutOfRangeException(nameof(target))
            };

            var transitionInfo = transition as NavigationTransitionInfo ?? new SlideNavigationTransitionInfo();
            var result = ContentFrame.Navigate(pageType, target, transitionInfo);

            return Task.FromResult(result);
        }
    }
}
