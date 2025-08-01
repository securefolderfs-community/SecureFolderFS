using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;

namespace SecureFolderFS.Maui.Views.Modals.Wizard
{
    public partial class SourceSelectionWizardPage : BaseModalPage
    {
        public SourceSelectionWizardViewModel ViewModel { get; }

        public WizardOverlayViewModel OverlayViewModel { get; }
        
        public SourceSelectionWizardPage(SourceSelectionWizardViewModel viewModel, WizardOverlayViewModel overlayViewModel)
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

