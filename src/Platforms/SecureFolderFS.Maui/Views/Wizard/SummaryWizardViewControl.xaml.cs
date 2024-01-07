using SecureFolderFS.Sdk.ViewModels.Views.Wizard2;

namespace SecureFolderFS.Maui.Views.Wizard
{
    public partial class SummaryWizardViewControl : ContentView
    {
        public SummaryWizardViewModel ViewModel { get; }

        public SummaryWizardViewControl(SummaryWizardViewModel viewModel)
        {
            ViewModel = viewModel;
            BindingContext = this;

            InitializeComponent();
        }
    }
}
