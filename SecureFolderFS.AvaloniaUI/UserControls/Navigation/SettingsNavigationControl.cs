using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.AvaloniaUI.Views.Settings;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <inheritdoc cref="ContentNavigationControl"/>
    internal sealed class SettingsNavigationControl : ContentNavigationControl
    {
        /// <inheritdoc/>
        public override Dictionary<Type, Type> TypeBinding { get; } = new()
        {
            { typeof(GeneralSettingsViewModel), typeof(GeneralSettingsPage) },
            { typeof(PreferencesSettingsViewModel), typeof(PreferencesSettingsPage) },
            { typeof(PrivacySettingsViewModel), typeof(PrivacySettingsPage) },
            { typeof(AboutSettingsViewModel), typeof(AboutSettingsPage) }
        };

        /// <inheritdoc/>
        protected override Task<bool> NavigateContentAsync(Type pageType, object parameter, NavigationTransition? transition)
        {
            transition ??= new EntranceNavigationTransition();
            return SetContentAsync(pageType, parameter, transition);
        }
    }
}