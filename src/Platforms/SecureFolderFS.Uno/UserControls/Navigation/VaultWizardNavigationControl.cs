using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Uno.Views.VaultWizard;

namespace SecureFolderFS.Uno.UserControls.Navigation
{
    /// <inheritdoc cref="FrameNavigationControl"/>
    internal sealed partial class VaultWizardNavigationControl : FrameNavigationControl
    {
        /// <inheritdoc/>
        public override Dictionary<Type, Type> TypeBinding { get; } = new()
        {
            { typeof(MainWizardViewModel), typeof(MainWizardPage) },
            { typeof(CredentialsWizardViewModel), typeof(CredentialsWizardPage) },
            { typeof(RecoveryWizardViewModel), typeof(RecoveryWizardPage) },
            { typeof(SummaryWizardViewModel), typeof(SummaryWizardPage) }
        };

        /// <inheritdoc/>
        protected override bool NavigateFrame(Type pageType, object parameter)
        {
            var transitionInfo = new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight };
            return ContentFrame.Navigate(pageType, parameter, transitionInfo);
        }
    }
}
