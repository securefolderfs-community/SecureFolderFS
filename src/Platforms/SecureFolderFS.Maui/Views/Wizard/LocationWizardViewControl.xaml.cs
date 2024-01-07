using SecureFolderFS.Sdk.ViewModels.Views.Wizard2;

namespace SecureFolderFS.Maui.Views.Wizard
{
    public partial class LocationWizardViewControl : ContentView
    {
        public LocationWizardViewModel ViewModel { get; set; }

        public LocationWizardViewControl(LocationWizardViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }
    }
}
