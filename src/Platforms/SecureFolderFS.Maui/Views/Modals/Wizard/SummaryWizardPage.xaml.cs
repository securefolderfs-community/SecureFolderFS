using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;

namespace SecureFolderFS.Maui.Views.Modals.Wizard
{
    public partial class SummaryWizardPage : BaseModalPage
    {
        public SummaryWizardViewModel ViewModel { get; }

        public WizardOverlayViewModel OverlayViewModel { get; }

        public SummaryWizardPage(SummaryWizardViewModel viewModel, WizardOverlayViewModel overlayViewModel)
        {
            ViewModel = viewModel;
            OverlayViewModel = overlayViewModel;
            BindingContext = this;

            _ = new MauiIcons.Core.MauiIcon(); // Workaround for XFC0000
            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override bool OnBackButtonPressed()
        {
            OverlayViewModel.CancellationCommand.Execute(null);
            return true;
        }

        /// <inheritdoc/>
        protected override void OnAppearing()
        {
            OverlayViewModel.CurrentViewModel = ViewModel;
            base.OnAppearing();
        }
    }
}
