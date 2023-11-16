using SecureFolderFS.AvaloniaUI.Views.Settings;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using System;
using System.Collections.Generic;
using FluentAvalonia.UI.Media.Animation;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <inheritdoc cref="FrameNavigationControl"/>
    internal sealed class SettingsNavigationControl : FrameNavigationControl
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
        protected override bool NavigateFrame(Type pageType, object parameter, NavigationTransitionInfo? transitionInfo)
        {
            transitionInfo ??= new EntranceNavigationTransitionInfo();
            return ContentFrame.Navigate(pageType, parameter, transitionInfo);
        }
    }
}