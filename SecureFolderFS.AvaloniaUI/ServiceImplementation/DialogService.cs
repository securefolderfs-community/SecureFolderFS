using SecureFolderFS.AvaloniaUI.Dialogs;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    /// <inheritdoc cref="IDialogService"/>
    internal sealed class DialogService : IDialogService
    {
        private readonly Dictionary<Type, Func<IDialog>> _dialogs;

        public DialogService()
        {
            _dialogs = new()
            {
                { typeof(ChangelogDialogViewModel), () => new ChangelogDialog() },
                { typeof(LicensesDialogViewModel), () => new LicensesDialog() },
                { typeof(SettingsDialogViewModel), () => new SettingsDialog() },
                { typeof(VaultWizardDialogViewModel), () => new VaultWizardDialog() },
                { typeof(PasswordChangeDialogViewModel), () => new PasswordChangeDialog() }
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

            return dialog;
        }

        /// <inheritdoc/>
        public void ReleaseDialog()
        {
            return; // TODO: Implement dialog releasing
        }
    }
}