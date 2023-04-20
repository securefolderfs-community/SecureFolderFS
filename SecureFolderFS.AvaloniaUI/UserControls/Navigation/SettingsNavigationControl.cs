using SecureFolderFS.AvaloniaUI.Animations.Transitions;
using System;
using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.AvaloniaUI.Views.Settings;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <inheritdoc cref="ContentNavigationControl"/>
    internal sealed class SettingsNavigationControl : ContentNavigationControl
    {
        /// <inheritdoc/>
        public override async Task<bool> NavigateAsync<TTarget, TTransition>(TTarget target, TTransition? transition = default) where TTransition : class
        {
            var pageType = target switch
            {
                GeneralSettingsViewModel => typeof(GeneralSettingsPage),
                PreferencesSettingsViewModel => typeof(PreferencesSettingsPage),
                PrivacySettingsViewModel => typeof(PrivacySettingsPage),
                AboutSettingsViewModel => typeof(AboutSettingsPage),
                _ => throw new ArgumentOutOfRangeException(nameof(target))
            };

            await Navigate(pageType, target, transition as TransitionBase);
            return true;
        }
    }
}