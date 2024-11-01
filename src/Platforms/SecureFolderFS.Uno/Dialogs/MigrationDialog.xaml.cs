using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Utils;
using SecureFolderFS.Uno.Extensions;

// To learn more about WinUI, the WinUI project structure,D
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class MigrationDialog : ContentDialog, IOverlayControl
    {
        public MigrationOverlayViewModel? ViewModel
        {
            get => DataContext.TryCast<MigrationOverlayViewModel>();
            set => DataContext = value;
        }

        public MigrationDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => (await base.ShowAsync()).ParseOverlayOption();

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (MigrationOverlayViewModel)viewable;
            ViewModel.StateChanged += ViewModel_StateChanged;
        }

        /// <inheritdoc/>
        public Task HideAsync()
        {
            Hide();
            return Task.CompletedTask;
        }

        private void ViewModel_StateChanged(object? sender, EventArgs e)
        {
            if (e is VaultUnlockedEventArgs)
            {
                AuthenticationView.Visibility = Visibility.Collapsed;
                MigrationView.Visibility = Visibility.Visible;
            }
            else if (e is MigrationCompletedEventArgs)
            {
                MigrationView.Visibility = Visibility.Collapsed;
                CompletedView.Visibility = Visibility.Visible;
            }
            else if (e is ErrorReportedEventArgs args)
            {
                if (args.Result.Exception is CryptographicException)
                {
                    if (AuthenticationView.ContentTemplateRoot is not IProgress<IResult?> reporter)
                        return;

                    reporter.Report(args.Result);
                }
                else
                {
                    MigrationView.Visibility = Visibility.Collapsed;
                    ErrorView.Visibility = Visibility.Visible;
                    ErrorView.ExceptionMessage = args.Result.GetMessage();
                }
            }
        }

        private async void MigrationDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            if (ViewModel is null)
                return;

            if (AuthenticationView.ContentTemplateRoot is not IWrapper<object?> wrapper)
                return;

            await ViewModel.AuthenticateMigrationCommand.ExecuteAsync(wrapper.Inner);
        }

        private void MigrationDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (ViewModel?.IsProgressing ?? false)
            {
                args.Cancel = true;
                return;
            }

            if (ViewModel is not null)
                ViewModel.StateChanged += ViewModel_StateChanged;
        }
    }
}
