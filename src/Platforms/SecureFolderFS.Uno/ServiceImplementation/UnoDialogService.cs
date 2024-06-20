using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.Utils;
using SecureFolderFS.Uno.Dialogs;
using SecureFolderFS.Uno.UserControls.Introduction;

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <inheritdoc cref="IOverlayService"/>
    public sealed class UnoDialogService : BaseOverlayService
    {
        private readonly Stack<IOverlayControl> _overlays = new();

        /// <inheritdoc/>
        protected override IOverlayControl GetOverlay(IViewable viewable)
        {
            IOverlayControl overlay = viewable switch
            {
                ChangelogOverlayViewModel => new ChangelogDialog(),
                LicensesOverlayViewModel => new LicensesDialog(),
                SettingsOverlayViewModel => new SettingsDialog(),
                WizardOverlayViewModel => new VaultWizardDialog(),
                PasswordChangeDialogViewModel => new PasswordChangeDialog(),
                ExplanationOverlayViewModel => new ExplanationDialog(),
                PreviewRecoveryOverlayViewModel => new PreviewRecoveryDialog(),

                // Unused
                PaymentDialogViewModel => new PaymentDialog(),
                AgreementDialogViewModel => new AgreementDialog(),
                IntroductionDialogViewModel => new IntroductionControl(),

                _ => throw new ArgumentException("Unknown viewable type.", nameof(viewable))
            };

            if (overlay is ContentDialog contentDialog)
                contentDialog.XamlRoot = App.Instance?.MainWindow?.Content?.XamlRoot;

            return overlay;
        }

        /// <inheritdoc/>
        public override async Task<IResult> ShowAsync(IViewable viewable)
        {
            if (_overlays.IsEmpty())
            {
                var overlay = GetOverlay(viewable);
                overlay.SetView(viewable);

                _overlays.Push(overlay);
                var result = await overlay.ShowAsync();
                _overlays.Pop();

                return result;
            }
            else
            {
                var current = _overlays.Pop();
                await current.HideAsync();

                var overlay = GetOverlay(viewable);
                overlay.SetView(viewable);

                _overlays.Push(overlay);
                var result = await overlay.ShowAsync();

                _overlays.Pop();
                await current.ShowAsync();

                return result;
            }
        }
    }
}
