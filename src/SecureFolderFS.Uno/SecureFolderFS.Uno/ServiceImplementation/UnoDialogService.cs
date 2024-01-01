using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.Utils;
using SecureFolderFS.Uno.Dialogs;
using SecureFolderFS.Uno.UserControls.Introduction;

#if WINDOWS
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation.Metadata;
#endif

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <inheritdoc cref="IOverlayService"/>
    public sealed class UnoDialogService : BaseOverlayService
    {
        private bool _isCurrentRemoved;

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

        /// <inheritdoc/>
        public override async Task<IResult> ShowAsync(IView view)
        {
            if (Overlays.IsEmpty())
                return await base.ShowAsync(view);

            var current = Overlays.Pop();
            current.Hide();
            _isCurrentRemoved = true;

            var overlay = GetOverlay(view);
            var result = await ShowOverlayAsync(overlay);
            if (!_isCurrentRemoved && !Overlays.IsEmpty())
                Overlays.Pop();

            Overlays.Push(current);
            await current.ShowAsync();
            Overlays.Pop();
            _isCurrentRemoved = false;

            return result;

            Task<IResult> ShowOverlayAsync(IOverlayControl overlay)
            {
                overlay.SetView(view);
                Overlays.Push(overlay);
                return overlay.ShowAsync();
            }
        }
    }
}
