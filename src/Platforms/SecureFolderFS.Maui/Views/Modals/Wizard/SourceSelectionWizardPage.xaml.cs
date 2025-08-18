using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources;

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

        private async void CollectionView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not BaseDataSourceWizardViewModel dataSourceViewModel)
                return;

            if (sender is not CollectionView collectionView)
                return;

            collectionView.SelectedItem = null;
            await ViewModel.SelectSourceCommand.ExecuteAsync(dataSourceViewModel);
        }
    }
}

