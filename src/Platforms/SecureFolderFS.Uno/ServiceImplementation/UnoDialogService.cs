using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly List<(IViewable, IOverlayControl)> _overlays = new();

        /// <inheritdoc/>
        protected override IOverlayControl GetOverlay(IViewable viewable)
        {
            IOverlayControl overlay = viewable switch
            {
                ChangelogOverlayViewModel => new ChangelogDialog(),
                LicensesOverlayViewModel => new LicensesDialog(),
                SettingsOverlayViewModel => new SettingsDialog(),
                WizardOverlayViewModel => new VaultWizardDialog(),
                CredentialsOverlayViewModel => new CredentialsDialog(),
                ExplanationOverlayViewModel => new ExplanationDialog(),
                PreviewRecoveryOverlayViewModel => new PreviewRecoveryDialog(),
                RecoveryOverlayViewModel => new RecoveryDialog(),
                MigrationOverlayViewModel => new MigrationDialog(),
                RecycleBinOverlayViewModel => new RecycleBinDialog(),

                // Unused
                PaymentOverlayViewModel => new PaymentDialog(),
                AgreementOverlayViewModel => new AgreementDialog(),
                IntroductionOverlayViewModel => new IntroductionControl(),

                _ => throw new ArgumentException("Unknown viewable type.", nameof(viewable))
            };

            if (overlay is ContentDialog contentDialog)
                contentDialog.XamlRoot = App.Instance?.MainWindow?.Content?.XamlRoot;

            return overlay;
        }

        /// <inheritdoc/>
        public override async Task<IResult> ShowAsync(IViewable viewable)
        {
            try
            {
                if (_overlays.IsEmpty())
                {
                    var overlay = GetOverlay(viewable);
                    overlay.SetView(viewable);
                    CurrentView = viewable;

                    return await ShowOverlayAsync(viewable, overlay);
                }
                else
                {
                    var (lastViewable, lastControl) = _overlays.Last();
                    await lastControl.HideAsync();

                    var overlay = GetOverlay(viewable);
                    overlay.SetView(viewable);
                    CurrentView = viewable;
                    var result = await ShowOverlayAsync(viewable, overlay);

                    CurrentView = lastViewable;
                    await ShowOverlayAsync(lastViewable, lastControl);

                    return result;
                }
            }
            finally
            {
                CurrentView = null;
            }

            async Task<IResult> ShowOverlayAsync(IViewable viewableToShow, IOverlayControl overlay)
            {
                _overlays.Add((viewableToShow, overlay));
                var result = await overlay.ShowAsync();
                _overlays.Remove((viewableToShow, overlay));

                return result;
            }
        }

        /// <inheritdoc/>
        public override async Task CloseAllAsync()
        {
            foreach (var (_, overlay) in _overlays.ToArray().Reverse())
                await overlay.HideAsync();

            _overlays.Clear();
        }
    }
}
