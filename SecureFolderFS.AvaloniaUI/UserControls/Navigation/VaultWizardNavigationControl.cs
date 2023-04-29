using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.AvaloniaUI.Views.VaultWizard;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.ExistingVault;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <inheritdoc cref="ContentNavigationControl"/>
    internal sealed class VaultWizardNavigationControl : ContentNavigationControl
    {
        /// <inheritdoc/>
        public override Dictionary<Type, Type> TypeBinding { get; } = new()
        {
            { typeof(MainWizardPageViewModel), typeof(MainWizardPage) },
            { typeof(ExistingLocationWizardViewModel), typeof(AddExistingWizardPage) },
            { typeof(NewLocationWizardViewModel), typeof(CreationPathWizardPage) },
            { typeof(PasswordWizardViewModel), typeof(PasswordWizardPage) },
            { typeof(EncryptionWizardViewModel), typeof(EncryptionWizardPage) },
            { typeof(SummaryWizardViewModel), typeof(SummaryWizardPage) },
        };

        /// <inheritdoc/>
        protected override Task<bool> NavigateContentAsync(Type pageType, object parameter, NavigationTransition? transition)
        {
            // Cache vault wizard page
            if (currentPage is not null)
                backStack.Push(currentPage.Value);

            transition ??= new SlideNavigationTransition(SlideNavigationTransition.Side.Right, Presenter.Bounds.Width, true);
            return SetContentAsync(pageType, parameter, transition);
        }
    }
}
