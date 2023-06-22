using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.UI.Utils;
using SecureFolderFS.WinUI.Dialogs;
using SecureFolderFS.WinUI.UserControls.Introduction;
using SecureFolderFS.WinUI.WindowViews;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Foundation.Metadata;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IDialogService"/>
    internal sealed class DialogService : IDialogService
    {
        private readonly Dictionary<Type, Func<IDialog>> _dialogs;
        
        public DialogService()
        {
            _dialogs = new()
            {
                { typeof(LicensesDialogViewModel), () => new LicensesDialog() },
                { typeof(SettingsDialogViewModel), () => new SettingsDialog() },
                { typeof(VaultWizardDialogViewModel), () => new VaultWizardDialog() },
                { typeof(PasswordChangeDialogViewModel), () => new PasswordChangeDialog() },
                { typeof(IntroductionDialogViewModel), () => new IntroductionControl() },
                { typeof(AgreementDialogViewModel), () => new AgreementDialog() },
                { typeof(PaymentDialogViewModel), () => new PaymentDialog() }
            };
        }

        /// <inheritdoc/>
        public IDialog GetDialog<TViewModel>(TViewModel viewModel)
            where TViewModel : class, INotifyPropertyChanged
        {
            if (!_dialogs.TryGetValue(typeof(TViewModel), out var initializer))
                throw new ArgumentException($"{typeof(TViewModel)} does not have an appropriate dialog associated with it.");

            var dialog = initializer();
            if (dialog is IDialog<TViewModel> dialog2)
                dialog2.ViewModel = viewModel;

            if (dialog is ContentDialog contentDialog && ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
                contentDialog.XamlRoot = MainWindow.Instance.Content.XamlRoot;

            if (dialog is IOverlayable overlayable)
                MainWindow.Instance.RootControl.Overlay.OverlayContent = overlayable.OverlayContent;

            return dialog;
        }
    }
}
