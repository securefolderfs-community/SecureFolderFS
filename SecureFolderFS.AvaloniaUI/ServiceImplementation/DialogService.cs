using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using SecureFolderFS.AvaloniaUI.Dialogs;
using SecureFolderFS.AvaloniaUI.Messages;
using SecureFolderFS.AvaloniaUI.WindowViews;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    /// <inheritdoc cref="IDialogService"/>
    internal sealed class DialogService : IDialogService
    {
        private readonly IReadOnlyDictionary<Type, Func<ContentDialog>> _dialogs;

        public DialogService()
        {
            _dialogs = new Dictionary<Type, Func<ContentDialog>>
            {
                { typeof(SettingsDialogViewModel), () => new SettingsDialog() },
                { typeof(VaultWizardDialogViewModel), () => new VaultWizardDialog() }
            };
        }

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

        public Task<DialogResult> ShowDialogAsync<TViewModel>(TViewModel viewModel) where TViewModel : class, INotifyPropertyChanged
        {
            WeakReferenceMessenger.Default.Send(new DialogShownMessage());
            return GetDialog(viewModel).ShowAsync();
        }
    }
}