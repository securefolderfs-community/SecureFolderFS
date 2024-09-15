using SecureFolderFS.Maui.Popups;
using SecureFolderFS.Maui.Prompts;
using SecureFolderFS.Maui.Views;
using SecureFolderFS.Maui.Views.Modals.Settings;
using SecureFolderFS.Maui.Views.Modals.Wizard;
using SecureFolderFS.Sdk.Services;
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
                RecoveryOverlayViewModel => new RecoveryPrompt(),
                PreviewRecoveryOverlayViewModel => new PreviewRecoveryPopup(),

                _ => throw new ArgumentException("Unknown viewable type.", nameof(viewable))
            };
        }
    }
}
