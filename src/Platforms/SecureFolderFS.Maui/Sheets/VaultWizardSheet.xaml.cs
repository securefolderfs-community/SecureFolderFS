using System.ComponentModel;
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
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            ViewModel.NavigationRequested += ViewModel_NavigationRequested;
        }

        /// <inheritdoc/>
        public Task HideAsync() => DismissAsync();

        private void VaultWizardSheet_Dismissed(object? sender, DismissOrigin e)
        {
            _tcs.SetResult(Result.Success);
        }

        private void VaultWizardSheet_Loaded(object? sender, EventArgs e)
        {
            ViewModel!.CurrentView = new MainWizardViewModel();
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WizardOverlayViewModel.CurrentView))
            {
                Presenter.Content = ViewModel?.CurrentView switch
                {
                    MainWizardViewModel => new MainWizardViewControl(ViewModel),
                    LocationWizardViewModel viewModel => new LocationWizardViewControl(viewModel),
                    _ => throw new ArgumentOutOfRangeException(nameof(ViewModel.CurrentView)),
                };

                ViewModel?.CurrentView?.OnAppearing();
            }
        }

        private void ViewModel_NavigationRequested(object? sender, NavigationRequestedEventArgs e)
        {
            Presenter.Content = e.Origin switch
            {
                // Main -> Location
                MainWizardViewModel viewModel => new LocationWizardViewControl(new(viewModel.CreationType, ViewModel!.VaultCollectionModel)),

                // Location -> Credentials
                // Credentials -> Recovery
                // Recovery -> Summary

                // Location -> Summary
                LocationWizardViewModel { CreationType: NewVaultCreationType.AddExisting } viewModel => new SummaryWizardViewControl(new(viewModel.SelectedFolder?.Name))
            };
        }
    }
}
