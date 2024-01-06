using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.UI.Utils;
using The49.Maui.BottomSheet;

namespace SecureFolderFS.Maui.Sheets
{
    public partial class VaultWizardSheet : BottomSheet, IOverlayControl
    {
        public VaultWizardDialogViewModel? ViewModel { get; set; }

        public VaultWizardSheet()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            await base.ShowAsync();
            return CommonResult.Success;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable) => ViewModel = (VaultWizardDialogViewModel)viewable;

        /// <inheritdoc/>
        public Task HideAsync() => DismissAsync();
    }
}
