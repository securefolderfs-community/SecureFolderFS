﻿using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault;
using SecureFolderFS.WinUI.Views.VaultWizard;
using System;
using System.Collections.Generic;

namespace SecureFolderFS.WinUI.UserControls.Navigation
{
    /// <inheritdoc cref="FrameNavigationControl"/>
    internal sealed class VaultWizardNavigationControl : FrameNavigationControl
    {
        /// <inheritdoc/>
        public override Dictionary<Type, Type> TypeBinding { get; } = new()
        {
            { typeof(MainWizardPageViewModel), typeof(MainWizardPage) },
            { typeof(AuthCreationWizardViewModel), typeof(AuthCreationWizardPage) },
            { typeof(RecoveryKeyWizardViewModel), typeof(RecoveryKeyWizardPage) },
            { typeof(SummaryWizardViewModel), typeof(SummaryWizardPage) }
        };

        /// <inheritdoc/>
        protected override bool NavigateFrame(Type pageType, object parameter, NavigationTransitionInfo? transitionInfo)
        {
            transitionInfo ??= new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight };
            return ContentFrame.Navigate(pageType, parameter, transitionInfo);
        }
    }
}
