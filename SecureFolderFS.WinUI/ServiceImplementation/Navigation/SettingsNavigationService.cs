using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using SecureFolderFS.WinUI.Views.Settings;
using System;

namespace SecureFolderFS.WinUI.ServiceImplementation.Navigation
{
    /// <inheritdoc cref="INavigationService"/>
    public sealed class SettingsNavigationService : FrameNavigationService
    {
        /// <inheritdoc/>
        protected override bool NavigateFrame(Frame frame, INavigationTarget target)
        {
            var pageType = target switch
            {
                GeneralSettingsViewModel => typeof(GeneralSettingsPage),
                PreferencesSettingsViewModel => typeof(PreferencesSettingsPage),
                PrivacySettingsViewModel => typeof(PrivacySettingsPage),
                AboutSettingsViewModel => typeof(AboutSettingsPage),
                _ => throw new ArgumentOutOfRangeException(nameof(target))
            };

            return frame.Navigate(pageType, target, new EntranceNavigationTransitionInfo());
        }
    }
}
