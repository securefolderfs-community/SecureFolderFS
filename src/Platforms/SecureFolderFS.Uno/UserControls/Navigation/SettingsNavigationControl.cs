using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using SecureFolderFS.Uno.Views.Settings;

namespace SecureFolderFS.Uno.UserControls.Navigation
{
    /// <inheritdoc cref="FrameNavigationControl"/>
    internal sealed partial class SettingsNavigationControl : FrameNavigationControl
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
        protected override bool NavigateFrame(Type pageType, object parameter)
        {
            var transitionInfo = new EntranceNavigationTransitionInfo();
            return ContentFrame.Navigate(pageType, parameter, transitionInfo);
        }
    }
}
