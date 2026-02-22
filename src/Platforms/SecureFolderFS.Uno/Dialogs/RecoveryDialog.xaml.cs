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
#if HAS_UNO_SKIA
        private bool _primaryClicked;
#endif

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
#if HAS_UNO_SKIA
            var result = await base.ShowAsync();
            return _primaryClicked ? ContentDialogResult.Primary.ParseOverlayOption() : result.ParseOverlayOption();
#else
            return (await base.ShowAsync()).ParseOverlayOption();
#endif
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
            var result = await ViewModel.RecoverAsync(default); 
            if (result.Successful)
            {
                _primaryClicked = true;
                Hide();
            }
#else
            var result = await ViewModel.RecoverAsync(default);
            args.Cancel = !result.Successful;
#endif
        }
    }
}
