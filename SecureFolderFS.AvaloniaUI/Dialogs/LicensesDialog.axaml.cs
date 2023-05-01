using System;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;

namespace SecureFolderFS.AvaloniaUI.Dialogs
{
    public sealed partial class LicensesDialog : ContentDialog, IDialog<LicensesDialogViewModel>, IStyleable
    {
        /// <inheritdoc/>
        public LicensesDialogViewModel? ViewModel
        {
            get => (LicensesDialogViewModel?)DataContext;
            set => DataContext = value;
        }

        public LicensesDialog()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public Type StyleKey => typeof(ContentDialog);

        /// <inheritdoc/>
        public async Task<DialogResult> ShowAsync() => (DialogResult)await base.ShowAsync();

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Licenses_Loaded(object? sender, RoutedEventArgs e)
        {
            _ = ViewModel.InitAsync();
        }
    }
}