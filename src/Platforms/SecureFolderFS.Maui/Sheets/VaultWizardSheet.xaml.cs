using OwlCore.Storage;
using SecureFolderFS.Maui.Views.Wizard;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.UI.Utils;
using The49.Maui.BottomSheet;

namespace SecureFolderFS.Maui.Sheets
{
    public partial class VaultWizardSheet : BottomSheet, IOverlayControl
    {
        private readonly TaskCompletionSource<IResult> _tcs;

        public WizardOverlayViewModel? ViewModel { get; set; }

        public VaultWizardSheet()
        {
            InitializeComponent();
            _tcs = new();
        }

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            await base.ShowAsync();
            return await _tcs.Task;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (WizardOverlayViewModel)viewable;
            ViewModel.NavigationRequested += ViewModel_NavigationRequested;
        }

        /// <inheritdoc/>
        public Task HideAsync() => DismissAsync();

        private void VaultWizardSheet_Dismissed(object? sender, DismissOrigin e)
        {
            if (ViewModel is not null)
                ViewModel.NavigationRequested -= ViewModel_NavigationRequested;

            _tcs.SetResult(Result.Success);
        }

        private void VaultWizardSheet_Loaded(object? sender, EventArgs e)
        {
            if (ViewModel is null)
                return;

            var mainWizardViewModel = new MainWizardViewModel();
            ViewModel.CurrentViewModel = mainWizardViewModel;
            ViewModel.CurrentViewModel.OnAppearing();
            Presenter.Content = new MainWizardViewControl(mainWizardViewModel, ViewModel);
        }

        private void ViewModel_NavigationRequested(object? sender, NavigationRequestedEventArgs e)
        {
            if (ViewModel is null)
                return;

            BaseWizardViewModel? nextViewModel = e.Origin switch
            {
                // Main -> Location
                MainWizardViewModel viewModel => new LocationWizardViewModel(viewModel.CreationType),

                // Location -> Credentials
                LocationWizardViewModel { CreationType: NewVaultCreationType.CreateNew, SelectedFolder: IModifiableFolder modifiableFolder } => new CredentialsWizardViewModel(modifiableFolder),

                // Credentials -> Recovery
                CredentialsWizardViewModel { Folder: { } folder } => new RecoveryWizardViewModel(folder, e.Result),

                // Recovery -> Summary
                RecoveryWizardViewModel { Folder: { } folder } => new SummaryWizardViewModel(folder, ViewModel.VaultCollectionModel),

                // Location -> Summary
                LocationWizardViewModel { CreationType: NewVaultCreationType.AddExisting, SelectedFolder: { } folder } => new SummaryWizardViewModel(folder, ViewModel.VaultCollectionModel),

                // TODO: Show error view model
                _ => null
            };

            Presenter.Content = nextViewModel switch
            {
                MainWizardViewModel viewModel => new MainWizardViewControl(viewModel, ViewModel),
                LocationWizardViewModel viewModel => new LocationWizardViewControl(viewModel, ViewModel),
                CredentialsWizardViewModel viewModel => new CredentialsWizardViewControl(viewModel, ViewModel),
                RecoveryWizardViewModel viewModel => new RecoveryWizardViewControl(viewModel, ViewModel),
                SummaryWizardViewModel viewModel => new SummaryWizardViewControl(viewModel),
                _ => null
            };
                
            ViewModel.CurrentViewModel = nextViewModel;
            ViewModel.CurrentViewModel?.OnAppearing();
        }
    }
}
