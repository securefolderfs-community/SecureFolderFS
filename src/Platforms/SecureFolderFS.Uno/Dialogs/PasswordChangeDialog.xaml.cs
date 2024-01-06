using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class PasswordChangeDialog : ContentDialog, IOverlayControl
    {
        public PasswordChangeDialogViewModel ViewModel
        {
            get => (PasswordChangeDialogViewModel)DataContext;
            set => DataContext = value;
        }

        public PasswordChangeDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => DialogExtensions.ResultFromDialogOption((DialogOption)await base.ShowAsync());

        /// <inheritdoc/>
        public void SetView(IViewable viewable) => ViewModel = (PasswordChangeDialogViewModel)viewable;

        /// <inheritdoc/>
        public Task HideAsync()
        {
            Hide();
            return Task.CompletedTask;
        }

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
            ViewModel.PrimaryButtonEnabled = !string.IsNullOrWhiteSpace(FirstPassword.PasswordInput.Password) && FirstPassword.Equals(SecondPassword);
        }
    }
}
