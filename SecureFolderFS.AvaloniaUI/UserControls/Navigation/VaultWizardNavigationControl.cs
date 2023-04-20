using SecureFolderFS.AvaloniaUI.Animations.Transitions;
using System;
using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.AvaloniaUI.Views.VaultWizard;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <inheritdoc cref="ContentNavigationControl"/>
    internal sealed class VaultWizardNavigationControl : ContentNavigationControl
    {
        /// <inheritdoc/>
        public override async Task<bool> NavigateAsync<TTarget, TTransition>(TTarget target, TTransition? transition = default) where TTransition : class
        {
            var pageType = target switch
            {
                MainWizardPageViewModel => typeof(MainWizardPage),
                VaultWizardSelectLocationViewModel => typeof(AddExistingWizardPage),
                VaultWizardCreationPathViewModel => typeof(CreationPathWizardPage),
                PasswordWizardViewModel => typeof(PasswordWizardPage),
                EncryptionWizardViewModel => typeof(EncryptionWizardPage),
                SummaryWizardViewModel => typeof(SummaryWizardPage),
                _ => throw new ArgumentOutOfRangeException(nameof(target))
            };

            var transitionInfo = transition as TransitionBase
                                 ?? new SlideNavigationTransition(SlideNavigationTransition.Side.Right, ContentPresenter.Bounds.Width, true);

            await Navigate(pageType, target, transitionInfo);
            return true;
        }
    }
}
