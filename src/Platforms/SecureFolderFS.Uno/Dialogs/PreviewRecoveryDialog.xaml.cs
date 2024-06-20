using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class PreviewRecoveryDialog : ContentDialog, IOverlayControl
    {
        public PreviewRecoveryOverlayViewModel? ViewModel
        {
            get => DataContext.TryCast<PreviewRecoveryOverlayViewModel>();
            set => DataContext = value;
        }

        public PreviewRecoveryDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => ((DialogOption)await base.ShowAsync()).ParseDialogOption();

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (PreviewRecoveryOverlayViewModel)viewable;
            ViewModel.OnAppearing();
        }

        /// <inheritdoc/>
        public Task HideAsync()
        {
            ViewModel?.OnDisappearing();
            Hide();
            return Task.CompletedTask;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (ViewModel is null)
                return;

            args.Cancel = true;
            ViewModel.LoginViewModel.ProvideCredentialsCommand?.Execute(null);
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            ViewModel?.OnDisappearing();
        }
    }
}
