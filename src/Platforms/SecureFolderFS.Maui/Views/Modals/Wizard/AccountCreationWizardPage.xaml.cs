using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources;

namespace SecureFolderFS.Maui.Views.Modals.Wizard
{
    public partial class AccountCreationWizardPage : BaseModalPage
    {
        public AccountCreationWizardViewModel ViewModel { get; }

        public WizardOverlayViewModel OverlayViewModel { get; }
        
        public AccountCreationWizardPage(AccountCreationWizardViewModel viewModel, WizardOverlayViewModel overlayViewModel)
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

