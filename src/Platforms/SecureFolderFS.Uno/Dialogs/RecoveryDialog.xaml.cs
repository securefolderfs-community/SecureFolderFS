using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Utils;
using SecureFolderFS.Uno.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class RecoveryDialog : ContentDialog, IOverlayControl
    {
        private bool _primaryClicked;

        public RecoveryOverlayViewModel? ViewModel
        {
            get => DataContext.TryCast<RecoveryOverlayViewModel>();
            set => DataContext = value;
        }

        public RecoveryDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync()
        {
            var result = await base.ShowAsync();
            return _primaryClicked ? ContentDialogResult.Primary.ParseOverlayOption() : result.ParseOverlayOption();
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (RecoveryOverlayViewModel)viewable;
            ViewModel.OnAppearing();
        }

        /// <inheritdoc/>
        public Task HideAsync()
        {
            Hide();
            return Task.CompletedTask;
        }

        private async void RecoveryDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (ViewModel is null)
                return;

#if HAS_UNO_SKIA
            args.Cancel = true;
            if (await ViewModel.RecoverAsync(default))
            {
                _primaryClicked = true;
                Hide();
            }
#else
            args.Cancel = !await ViewModel.RecoverAsync(default);
#endif
        }
    }
}
