using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Dialogs;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.WinUI.Dialogs;
using SecureFolderFS.WinUI.WindowViews;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    internal sealed class DialogService : IDialogService
    {
        private readonly IReadOnlyDictionary<Type, Func<ContentDialog>> _dialogs;

        public DialogService()
        {
            _dialogs = new Dictionary<Type, Func<ContentDialog>>()
            {
                { typeof(SettingsDialogViewModel), () => new SettingsDialog() },
                { typeof(VaultWizardDialogViewModel), () => new VaultWizardDialog() }
            };
        }

        public IDialog<TViewModel> GetDialog<TViewModel>(TViewModel viewModel)
            where TViewModel : INotifyPropertyChanged
        {
            if (!_dialogs.TryGetValue(typeof(TViewModel), out var initializer))
            {
                throw new ArgumentException($"{typeof(TViewModel)} does not have an appropriate dialog associated with it.");
            }

            var contentDialog = initializer();

            if (contentDialog is not IDialog<TViewModel> dialog)
            {
                throw new NotSupportedException($"The dialog does not implement {typeof(IDialog<TViewModel>)}.");
            }

            dialog.ViewModel = viewModel;
            contentDialog.XamlRoot = MainWindow.Instance!.Content.XamlRoot;

            return dialog;
        }

        public Task<DialogResult> ShowDialogAsync<TViewModel>(TViewModel viewModel)
            where TViewModel : INotifyPropertyChanged
        {
            return GetDialog(viewModel).ShowAsync();
        }
    }
}
