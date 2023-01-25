using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
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
        /// <inheritdoc cref="Frame.CanGoBack"/>
        public bool CanGoBack => ContentFrame.CanGoBack;

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
                VaultWizardSelectLocationViewModel => typeof(AddExistingWizardPage),
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
