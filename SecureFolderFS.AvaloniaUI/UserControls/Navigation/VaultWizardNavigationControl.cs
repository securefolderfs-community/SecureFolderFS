using System;
using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.AvaloniaUI.Views.VaultWizard;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard.ExistingVault;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard.NewVault;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <inheritdoc cref="NavigationControl"/>
    internal sealed class VaultWizardNavigationControl : NavigationControl
    {
        /// <inheritdoc/>
        public override void Receive(BackNavigationRequestedMessage message)
        {
            if (CanGoBack)
                GoBack();
        }

        /// <inheritdoc/>
        public override void Navigate<TViewModel>(TViewModel viewModel, NavigationTransition? transition)
        {
            transition ??= new SlideNavigationTransition(SlideNavigationTransition.Side.Right, ContentPresenter.Bounds.Width, true);
            var pageType = viewModel switch
            {
                MainVaultWizardPageViewModel => typeof(MainWizardPage),
                VaultWizardSelectLocationViewModel => typeof(AddExistingWizardPage),
                VaultWizardCreationPathViewModel => typeof(CreationPathWizardPage),
                VaultWizardPasswordViewModel => typeof(PasswordWizardPage),
                VaultWizardEncryptionViewModel => typeof(EncryptionWizardPage),
                VaultWizardSummaryViewModel => typeof(SummaryWizardPage),
                _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
            };

            Navigate(pageType, viewModel, transition);
        }
    }
}
