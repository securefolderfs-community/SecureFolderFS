using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;

namespace SecureFolderFS.Maui.Views.Wizard
{
    public partial class CredentialsWizardViewControl : ContentView
    {
        public CredentialsWizardViewModel ViewModel { get; }

        public WizardOverlayViewModel OverlayViewModel { get; }

        public CredentialsWizardViewControl(CredentialsWizardViewModel viewModel, WizardOverlayViewModel overlayViewModel)
        {
            ViewModel = viewModel;
            OverlayViewModel = overlayViewModel;
            BindingContext = this;

            InitializeComponent();
        }
    }
}
