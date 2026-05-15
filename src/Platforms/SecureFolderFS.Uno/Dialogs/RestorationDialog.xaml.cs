using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class RestorationDialog : ContentDialog, IOverlayControl
    {
        private TaskCompletionSource<IResult> _tcs = new();
        
        public VaultRestorationOverlayViewModel? ViewModel
        {
            get => DataContext.TryCast<VaultRestorationOverlayViewModel>();
            set => DataContext = value;
        }

        public RestorationDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync()
        {
            await base.ShowAsync();
            return await _tcs.Task;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (VaultRestorationOverlayViewModel)viewable;
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
            args.Cancel = true;
            if (ViewModel is null)
                return;

            var result = await ViewModel.RestoreAsync();
            if (!result.Successful)
                return;

            _tcs.TrySetResult(result);
            Hide();
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            _tcs.TrySetResult(Result<DialogOption>.Failure(DialogOption.Cancel));
            ViewModel?.OnDisappearing();
        }
    }
}
