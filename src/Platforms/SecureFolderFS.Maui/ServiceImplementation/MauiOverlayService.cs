using SecureFolderFS.Maui.Popups;
using SecureFolderFS.Maui.Prompts;
using SecureFolderFS.Maui.Sheets;
using SecureFolderFS.Maui.Views;
using SecureFolderFS.Maui.Views.Modals.DeviceLink;
using SecureFolderFS.Maui.Views.Modals.Settings;
using SecureFolderFS.Maui.Views.Modals.Vault;
using SecureFolderFS.Maui.Views.Modals.Wizard;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.Maui.ServiceImplementation
{
    /// <inheritdoc cref="IOverlayService"/>
    internal sealed class MauiOverlayService : BaseOverlayService
    {
        /// <inheritdoc/>
        protected override IOverlayControl GetOverlay(IViewable viewable)
        {
            // TODO: Check if it's better to use Shell.Current.TryCast<AppShell>().CurrentPage
            var navigation = MainPage.Instance!.Navigation;
            return viewable switch
            {
                WizardOverlayViewModel => new MainWizardPage(navigation),
                SettingsOverlayViewModel => new SettingsPage(navigation),
                PreviewerOverlayViewModel => new FilePreviewModalPage(navigation),
                RecycleBinOverlayViewModel => new RecycleBinModalPage(navigation),
                IntroductionOverlayViewModel => new IntroductionPage(navigation),
                LayoutsViewModel => new ViewOptionsSheetFragment(),
                RenameOverlayViewModel => new RenamePrompt(),
                NewItemOverlayViewModel => new NewItemPrompt(),
                RecoveryOverlayViewModel => new RecoveryPrompt(),
                MessageOverlayViewModel => new MessagePrompt(),
                PropertiesOverlayViewModel => new PropertiesPopup(),
                CredentialsOverlayViewModel => new CredentialsPopup(),
                StorableTypeOverlayViewModel => new StorableTypePrompt(),
                PreviewRecoveryOverlayViewModel => new PreviewRecoveryPopup(),
                DeviceLinkRequestOverlayViewModel => new DeviceLinkRequestPage(navigation),
                DeviceLinkCredentialsOverlayViewModel => new DeviceLinkCredentialsPage(navigation),

                _ => throw new ArgumentException("Unknown viewable type.", nameof(viewable))
            };
        }
    }
}
