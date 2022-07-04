using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;
using SecureFolderFS.WinUI.Views.VaultWizard;

namespace SecureFolderFS.WinUI.UserControls.Navigation
{
    /// <inheritdoc cref="NavigationControl"/>
    internal sealed class VaultWizardNavigationControl : NavigationControl
    {
        /// <inheritdoc cref="Frame.CanGoBack"/>
        public bool CanGoBack
        {
            get => ContentFrame.CanGoBack;
        }

        /// <inheritdoc/>
        public override void Receive(BackNavigationRequestedMessage message)
        {
            if (ContentFrame.CanGoBack)
                ContentFrame.GoBack();
        }

        /// <inheritdoc/>
        public override void Navigate<TViewModel>(TViewModel viewModel, NavigationTransitionInfo? transitionInfo)
        {
            transitionInfo ??= new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight };
            var pageType = viewModel switch
            {
                MainVaultWizardPageViewModel => typeof(MainWizardPage),
                VaultWizardAddExistingViewModel => typeof(AddExistingWizardPage),
                VaultWizardCreationPathViewModel => typeof(CreationPathWizardPage),
                VaultWizardPasswordViewModel => typeof(PasswordWizardPage),
                VaultWizardEncryptionViewModel => typeof(EncryptionWizardPage),
                VaultWizardSummaryViewModel => typeof(SummaryWizardPage),
                _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
            };

            ContentFrame.Navigate(pageType, viewModel, transitionInfo);
        }
    }
}
