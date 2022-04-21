using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Backend.Dialogs;
using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Backend.Services;
using SecureFolderFS.Backend.ViewModels.Dialogs;
using SecureFolderFS.WinUI.Dialogs;
using SecureFolderFS.WinUI.WindowViews;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    internal sealed class DialogService : IDialogService
    {
        private readonly Dictionary<Type, Func<ContentDialog>> _dialogs;

        public DialogService()
        {
            this._dialogs = new()
            {
                { typeof(SettingsDialogViewModel), () => new SettingsDialog() },
                { typeof(VaultWizardDialogViewModel), () => new VaultWizardDialog() },
                { typeof(DokanyDialogViewModel), () => new DokanyDialog() }
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

        public Task<DialogResult> ShowDialog<TViewModel>(TViewModel viewModel)
            where TViewModel : INotifyPropertyChanged
        {
            return GetDialog<TViewModel>(viewModel).ShowAsync();
        }
    }
}
