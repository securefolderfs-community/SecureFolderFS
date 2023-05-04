using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.Dialogs
{
    public sealed partial class PasswordChangeDialog : ContentDialog, IStyleable, IDialog<PasswordChangeDialogViewModel>
    {
        /// <inheritdoc/>
        public PasswordChangeDialogViewModel ViewModel
        {
            get => (PasswordChangeDialogViewModel)DataContext;
            set => DataContext = value;
        }

        public PasswordChangeDialog()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public Type StyleKey => typeof(ContentDialog);

        /// <inheritdoc/>
        public async Task<DialogResult> ShowAsync() => (DialogResult)await base.ShowAsync();

        private async void PasswordChangeDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;

            var existingPassword = ExistingPassword.GetPassword();
            var newPassword = FirstPassword.GetPassword();

            if (existingPassword is null || newPassword is null)
                return;

            if (await ViewModel.ChangePasswordAsync(new(existingPassword, newPassword), CancellationToken.None))
                Hide();
        }

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ViewModel.PrimaryButtonEnabled = !string.IsNullOrWhiteSpace(FirstPassword.PasswordInput.Text) && FirstPassword.Equals(SecondPassword);
        }
    }
}
