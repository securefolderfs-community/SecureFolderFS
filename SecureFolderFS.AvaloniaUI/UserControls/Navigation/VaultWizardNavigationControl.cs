using SecureFolderFS.AvaloniaUI.Animations.Transitions;
using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.AvaloniaUI.Views.VaultWizard;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard.ExistingVault;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard.NewVault;
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
                MainVaultWizardPageViewModel => typeof(MainWizardPage),
                VaultWizardSelectLocationViewModel => typeof(AddExistingWizardPage),
                VaultWizardCreationPathViewModel => typeof(CreationPathWizardPage),
                VaultWizardPasswordViewModel => typeof(PasswordWizardPage),
                VaultWizardEncryptionViewModel => typeof(EncryptionWizardPage),
                VaultWizardSummaryViewModel => typeof(SummaryWizardPage),
                _ => throw new ArgumentOutOfRangeException(nameof(target))
            };

            var transitionInfo = transition as TransitionBase
                                 ?? new SlideNavigationTransition(SlideNavigationTransition.Side.Right, SlideNavigationTransition.SmallOffset);

            await Navigate(pageType, target, transitionInfo);
            return true;
        }
    }
}
