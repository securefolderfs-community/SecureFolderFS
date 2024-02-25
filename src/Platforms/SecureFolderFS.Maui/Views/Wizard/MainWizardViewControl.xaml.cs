using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;

namespace SecureFolderFS.Maui.Views.Wizard
{
    public partial class MainWizardViewControl : ContentView
    {
        private readonly WizardOverlayViewModel _overlayViewModel;

        public MainWizardViewModel? ViewModel { get; set; }

        public MainWizardViewControl(WizardOverlayViewModel viewModel)
        {
            _overlayViewModel = viewModel;
            ViewModel = viewModel.CurrentView as MainWizardViewModel;
            BindingContext = this;

            InitializeComponent();
        }

        private async void Existing_Clicked(object? sender, EventArgs e)
        {
            if (ViewModel is not null)
                ViewModel.CreationType = NewVaultCreationType.AddExisting;

            await _overlayViewModel.ContinuationCommand.ExecuteAsync(null);
        }

        private async void New_Clicked(object? sender, EventArgs e)
        {
            if (ViewModel is not null)
                ViewModel.CreationType = NewVaultCreationType.CreateNew;

            await _overlayViewModel.ContinuationCommand.ExecuteAsync(null);
        }
    }
}
