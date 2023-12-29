using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.Utils;
using SecureFolderFS.Uno.Dialogs;
using SecureFolderFS.Uno.UserControls.Introduction;
using Windows.Foundation.Metadata;

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <inheritdoc cref="IOverlayService"/>
    public sealed class UnoDialogService : BaseOverlayService
    {
        /// <inheritdoc/>
        protected override IOverlayControl GetOverlay(IView view)
        {
            IOverlayControl overlay = view switch
            {
                ChangelogDialogViewModel => new ChangelogDialog(),
                LicensesDialogViewModel => new LicensesDialog(),
                SettingsDialogViewModel => new SettingsDialog(),
                VaultWizardDialogViewModel => new VaultWizardDialog(),
                PasswordChangeDialogViewModel => new PasswordChangeDialog(),
                ExplanationDialogViewModel => new ExplanationDialog(),

                // Unused
                PaymentDialogViewModel => new PaymentDialog(),
                AgreementDialogViewModel => new AgreementDialog(),
                IntroductionDialogViewModel => new IntroductionControl()
            };

#if WINDOWS
            if (overlay is ContentDialog contentDialog && ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
                contentDialog.XamlRoot = App.Instance?.MainWindow?.Content.XamlRoot;
#endif

            return overlay;
        }
    }
}
