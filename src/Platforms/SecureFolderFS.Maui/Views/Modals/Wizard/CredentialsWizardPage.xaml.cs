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
        }

        /// <inheritdoc/>
        protected override void OnAppearing()
        {
            OverlayViewModel.CurrentViewModel = ViewModel;
            base.OnAppearing();
        }

        private void Shortening_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (sender is not Entry entry)
                return;

            if (!int.TryParse(entry.Text, out var value))
                value = 0;
            
            value = Math.Max(0, Math.Min(value, 250));
            entry.Text = value.ToString();
            ViewModel.ShorteningThreshold = value;
        }
    }
}
