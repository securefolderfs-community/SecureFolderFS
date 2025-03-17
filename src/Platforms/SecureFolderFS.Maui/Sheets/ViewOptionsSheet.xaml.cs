using SecureFolderFS.Sdk.ViewModels.Controls.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Utils;
using The49.Maui.BottomSheet;

namespace SecureFolderFS.Maui.Sheets
{
    public partial class ViewOptionsSheet : BottomSheet, IOverlayControl
    {
        private readonly TaskCompletionSource<IResult> _tcs;

        public ViewOptionsSheet()
        {
            _tcs = new();
            InitializeComponent();
        }

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            await base.ShowAsync();
            return await _tcs.Task;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (ViewOptionsViewModel)viewable;
        }

        /// <inheritdoc/>
        public Task HideAsync()
        {
            return DismissAsync();
        }

        private void ViewOptionsSheet_Dismissed(object? sender, DismissOrigin e)
        {
            _tcs.SetResult(Result.Success);
        }

        public ViewOptionsViewModel? ViewModel
        {
            get => (ViewOptionsViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(ViewOptionsViewModel), typeof(ViewOptionsSheet), null);
    }
}

