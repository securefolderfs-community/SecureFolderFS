using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;

namespace SecureFolderFS.Maui.Views.Wizard
{
    public partial class RecoveryWizardViewControl : ContentView
    {
        public RecoveryWizardViewModel ViewModel { get; }

        public WizardOverlayViewModel OverlayViewModel { get; }

        public RecoveryWizardViewControl(RecoveryWizardViewModel viewModel, WizardOverlayViewModel overlayViewModel)
        {
            ViewModel = viewModel;
            OverlayViewModel = overlayViewModel;
            BindingContext = this;
            viewModel.RevealMasterKeyCommand.Execute(null);

            InitializeComponent();
        }
    }
}
