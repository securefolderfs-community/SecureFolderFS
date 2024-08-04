using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;

namespace SecureFolderFS.Maui.Views.Modals.Wizard
{
    public partial class RecoveryWizardPage : BaseModalPage
    {
        public RecoveryWizardViewModel ViewModel { get; }

        public WizardOverlayViewModel OverlayViewModel { get; }

        public RecoveryWizardPage(RecoveryWizardViewModel viewModel, WizardOverlayViewModel overlayViewModel)
        {
            ViewModel = viewModel;
            OverlayViewModel = overlayViewModel;
            BindingContext = this;

            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void OnAppearing()
        {
            OverlayViewModel.CurrentViewModel = ViewModel;
            base.OnAppearing();
        }
    }
}
