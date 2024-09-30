using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Sdk.ViewModels.Views.Credentials;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Utils;
using SecureFolderFS.Uno.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class CredentialsDialog : ContentDialog, IOverlayControl
    {
        public CredentialsOverlayViewModel? ViewModel
        {
            get => DataContext.TryCast<CredentialsOverlayViewModel>();
            set => DataContext = value;
        }

        public CredentialsDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => (await base.ShowAsync()).ParseOverlayOption();

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (CredentialsOverlayViewModel)viewable;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            ViewModel.OnAppearing();
        }

        /// <inheritdoc/>
        public Task HideAsync()
        {
            Hide();
            return Task.CompletedTask;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (ViewModel is null)
                return;

            args.Cancel = true;
            if (ViewModel.SelectedViewModel is LoginViewModel loginViewModel)
            {
                loginViewModel.ProvideCredentialsCommand?.Execute(null);
            }
            else if (ViewModel.SelectedViewModel is CredentialsResetViewModel credentialsReset)
            {
                try
                {
                    await credentialsReset.ConfirmAsync(default);
                    await HideAsync();
                }
                catch (Exception ex)
                {
                    // TODO: Report to user
                    _ = ex;
                }
            }
            else if (ViewModel.SelectedViewModel is CredentialsConfirmationViewModel credentialsConfirmation)
            {
                try
                {
                    if (credentialsConfirmation.IsRemoving)
                        await credentialsConfirmation.RemoveAsync(default);
                    else
                        await credentialsConfirmation.ConfirmAsync(default);
                        
                    await HideAsync();
                }
                catch (Exception ex)
                {
                    // TODO: Report to user
                    _ = ex;
                }
            }
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            ViewModel?.OnDisappearing();
        }

        private void ResetCredentialsLink_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private async void ActionBlockControl_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel is null)
                return;

            if (sender is not FrameworkElement { DataContext: AuthenticationViewModel selectedAuthentication })
                return;

            await ViewModel.SelectionViewModel.ItemSelectedCommand.ExecuteAsync(selectedAuthentication);
        }

        private async void GoBack_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel is null)
                return;

            if (ViewModel.SelectedViewModel is not CredentialsConfirmationViewModel confirmationViewModel)
                return;

            // We also need to revoke existing credentials if the user added and aborted
            if (confirmationViewModel.RegisterViewModel.CurrentViewModel is not null)
                await confirmationViewModel.RegisterViewModel.RevokeCredentialsAsync(default);

            ViewModel.SelectedViewModel = ViewModel.SelectionViewModel;
            ViewModel.PrimaryButtonText = null;
            ViewModel.Title = "SelectAuthentication".ToLocalized();

            await BackTitle.AnimateBackAsync(false);
        }

        private async void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (ViewModel is null)
                return;

            if (e.PropertyName == nameof(CredentialsOverlayViewModel.SelectedViewModel))
            {
                if (ViewModel.SelectedViewModel is CredentialsConfirmationViewModel)
                    await BackTitle.AnimateBackAsync(true);
            }
        }

        private void CredentialsDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            if (ViewModel is not null)
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
    }
}
