using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;

namespace SecureFolderFS.Maui.Views.Modals.Wizard
{
    public partial class CredentialsWizardPage : BaseModalPage
    {
        public CredentialsWizardViewModel ViewModel { get; }

        public WizardOverlayViewModel OverlayViewModel { get; }

        public CredentialsWizardPage(CredentialsWizardViewModel viewModel, WizardOverlayViewModel overlayViewModel)
        {
            ViewModel = viewModel;
            OverlayViewModel = overlayViewModel;
            BindingContext = this;

            InitializeComponent();
            _ = new MauiIcons.Core.MauiIcon(); // Workaround for XFC0000
        }

        /// <inheritdoc/>
        protected override void OnAppearing()
        {
            OverlayViewModel.CurrentViewModel = ViewModel;
            base.OnAppearing();
        }
    }
}
