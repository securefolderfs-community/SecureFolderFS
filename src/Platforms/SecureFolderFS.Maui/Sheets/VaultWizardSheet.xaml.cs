using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.UI.Utils;
using The49.Maui.BottomSheet;

namespace SecureFolderFS.Maui.Sheets
{
    public partial class VaultWizardSheet : BottomSheet, IOverlayControl
    {
        private readonly TaskCompletionSource<IResult> _tcs;

        public VaultWizardDialogViewModel? ViewModel { get; set; }

        public VaultWizardSheet()
        {
            InitializeComponent();
            _tcs = new();
        }

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            await base.ShowAsync();
            return await _tcs.Task;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable) => ViewModel = (VaultWizardDialogViewModel)viewable;

        /// <inheritdoc/>
        public Task HideAsync() => DismissAsync();

        private void VaultWizardSheet_Dismissed(object? sender, DismissOrigin e)
        {
            _tcs.SetResult(CommonResult.Success);
        }
    }
}
