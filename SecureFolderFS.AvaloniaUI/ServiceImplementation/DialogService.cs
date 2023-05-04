using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using SecureFolderFS.AvaloniaUI.Dialogs;
using SecureFolderFS.AvaloniaUI.Messages;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    /// <inheritdoc cref="IDialogService"/>
    internal sealed class DialogService : IDialogService
    {
        private readonly IReadOnlyDictionary<Type, Func<ContentDialog>> _dialogs;
        private IDialog? _currentDialog;

        public DialogService()
        {
            _dialogs = new Dictionary<Type, Func<ContentDialog>>
            {
                { typeof(LicensesDialogViewModel), () => new LicensesDialog() },
                { typeof(SettingsDialogViewModel), () => new SettingsDialog() },
                { typeof(VaultWizardDialogViewModel), () => new VaultWizardDialog() },
                { typeof(PasswordChangeDialogViewModel), () => new PasswordChangeDialog() }
            };
        }

        /// <inheritdoc/>
        public IDialog<TViewModel> GetDialog<TViewModel>(TViewModel viewModel) where TViewModel : class, INotifyPropertyChanged
        {
            if (!_dialogs.TryGetValue(typeof(TViewModel), out var initializer))
                throw new ArgumentException($"{typeof(TViewModel)} does not have an appropriate dialog associated with it.");

            var contentDialog = initializer();
            if (contentDialog is not IDialog<TViewModel> dialog)
                throw new NotSupportedException($"The dialog does not implement {typeof(IDialog<TViewModel>)}.");

            dialog.ViewModel = viewModel;
            return dialog;
        }

        /// <inheritdoc/>
        public Task<DialogResult> ShowDialogAsync<TViewModel>(TViewModel viewModel) where TViewModel : class, INotifyPropertyChanged
        {
            _currentDialog?.Hide();
            _currentDialog = GetDialog(viewModel);
            WeakReferenceMessenger.Default.Send(new DialogShownMessage());

            return _currentDialog.ShowAsync();
        }
    }
}