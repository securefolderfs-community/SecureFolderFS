using SecureFolderFS.Maui.Sheets;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.Maui.ServiceImplementation
{
    /// <inheritdoc cref="IOverlayService"/>
    internal sealed class SheetService : BaseOverlayService
    {
        /// <inheritdoc/>
        protected override IOverlayControl GetOverlay(IViewable viewable)
        {
            return viewable switch
            {
                WizardOverlayViewModel => new VaultWizardSheet(),
                _ => throw new ArgumentException("Unknown viewable type.", nameof(viewable))
            };
        }
    }
}
