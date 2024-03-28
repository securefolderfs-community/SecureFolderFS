using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;

namespace SecureFolderFS.Maui.Views.Wizard
{
    public partial class MainWizardViewControl : ContentView
    {
        public MainWizardViewModel ViewModel { get; }

        public WizardOverlayViewModel OverlayViewModel { get; }

        public MainWizardViewControl(MainWizardViewModel viewModel, WizardOverlayViewModel overlayViewModel)
        {
            ViewModel = viewModel;
            OverlayViewModel = overlayViewModel;
            BindingContext = this;

            InitializeComponent();
        }

        private async void Existing_Clicked(object? sender, EventArgs e)
        {
            ViewModel.CreationType = NewVaultCreationType.AddExisting;
            await OverlayViewModel.ContinuationCommand.ExecuteAsync(null);
        }

        private async void New_Clicked(object? sender, EventArgs e)
        {
            ViewModel.CreationType = NewVaultCreationType.CreateNew;
            await OverlayViewModel.ContinuationCommand.ExecuteAsync(null);
        }
    }
}
