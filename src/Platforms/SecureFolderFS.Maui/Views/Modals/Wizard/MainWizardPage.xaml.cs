using OwlCore.Storage;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Utils;

#if IOS
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;
#endif

namespace SecureFolderFS.Maui.Views.Modals.Wizard
{
    public partial class MainWizardPage : BaseModalPage, IOverlayControl
    {
        private readonly INavigation _sourceNavigation;
        private readonly TaskCompletionSource<IResult> _modalTcs;

        public MainWizardViewModel? ViewModel { get; private set; }

        public WizardOverlayViewModel? OverlayViewModel { get; private set; }

        public MainWizardPage(INavigation sourceNavigation)
        {
            _modalTcs = new();
            _sourceNavigation = sourceNavigation;
            BindingContext = this;

            InitializeComponent();
        }

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            // Using Shell to display modals is broken - each new page shown after a 'modal' page will be incorrectly displayed as another modal.
            // NavigationPage approach does not have this issue
#if ANDROID
            await _sourceNavigation.PushModalAsync(new NavigationPage(this)
            {
                BackgroundColor = Color.FromArgb("#80000000")
            });
#elif IOS
            var navigationPage = new NavigationPage(this);
            NavigationPage.SetIconColor(navigationPage, Color.FromArgb("#007bff"));
            navigationPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.PageSheet);
            await _sourceNavigation.PushModalAsync(navigationPage);
#endif

            return await _modalTcs.Task;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            OverlayViewModel = (WizardOverlayViewModel)viewable;
            ViewModel = new(OverlayViewModel.VaultCollectionModel);
            OverlayViewModel.NavigationRequested += ViewModel_NavigationRequested;
            OverlayViewModel.CurrentViewModel = ViewModel;
            Shell.Current.Navigated += Shell_Navigated;

            OnPropertyChanged(nameof(ViewModel));
            OnPropertyChanged(nameof(OverlayViewModel));
        }

        /// <inheritdoc/>
        public async Task HideAsync()
        {
            await Shell.Current.GoBackAsync(Navigation.NavigationStack.Count);
        }

        /// <inheritdoc/>
        protected override void OnAppearing()
        {
            if (OverlayViewModel is not null)
                OverlayViewModel.CurrentViewModel = ViewModel;
            
            base.OnAppearing();
        }

        private void Shell_Navigated(object? sender, ShellNavigatedEventArgs e)
        {
            if (e.Current.Location.OriginalString.Contains("NavigationPage"))
                return;

            Shell.Current.Navigated -= Shell_Navigated;
            if (OverlayViewModel is not null)
                OverlayViewModel.NavigationRequested -= ViewModel_NavigationRequested;
            
            _modalTcs.TrySetResult(OverlayViewModel?.CurrentViewModel is SummaryWizardViewModel
                ? Result.Success
                : Result.Failure(null));
        }

        private async void ViewModel_NavigationRequested(object? sender, Shared.EventArguments.NavigationRequestedEventArgs e)
        {
            if (OverlayViewModel is null)
                return;

            if (e is CloseNavigationRequestedEventArgs)
            {
                await HideAsync();
                return;
            }

            BaseWizardViewModel? nextViewModel = e.Origin switch
            {
                // Main -> Location
                MainWizardViewModel viewModel => new LocationWizardViewModel(OverlayViewModel.VaultCollectionModel, viewModel.CreationType),

                // Location -> Credentials
                LocationWizardViewModel { CreationType: NewVaultCreationType.CreateNew, SelectedFolder: IModifiableFolder modifiableFolder } => new CredentialsWizardViewModel(modifiableFolder),

                // Credentials -> Recovery
                CredentialsWizardViewModel credentials => new RecoveryWizardViewModel(credentials.Folder, (e as WizardNavigationRequestedEventArgs)?.Result),

                // Recovery -> Summary
                RecoveryWizardViewModel recovery => new SummaryWizardViewModel(recovery.Folder, OverlayViewModel.VaultCollectionModel),

                // Location -> Summary
                LocationWizardViewModel { CreationType: NewVaultCreationType.AddExisting, SelectedFolder: { } folder } => new SummaryWizardViewModel(folder, OverlayViewModel.VaultCollectionModel),

                // Close
                _ => null
            };

            if (nextViewModel is null)
            {
                _modalTcs.SetResult(e.Origin is SummaryWizardViewModel ? Result.Success : Result.Failure(null));
                await HideAsync();
                return;
            }

            var page = (BaseModalPage?)(nextViewModel switch
            {
                MainWizardViewModel _ => new MainWizardPage(_sourceNavigation),
                LocationWizardViewModel viewModel => new LocationWizardPage(viewModel, OverlayViewModel),
                CredentialsWizardViewModel viewModel => new CredentialsWizardPage(viewModel, OverlayViewModel),
                RecoveryWizardViewModel viewModel => new RecoveryWizardPage(viewModel, OverlayViewModel),
                SummaryWizardViewModel viewModel => new SummaryWizardPage(viewModel, OverlayViewModel),
                _ => null
            });

            await Navigation.PushAsync(page, true);
        }

        private async void Existing_Clicked(object? sender, EventArgs e)
        {
            if (ViewModel is null || OverlayViewModel is null)
                return;

            ViewModel.CreationType = NewVaultCreationType.AddExisting;
            await OverlayViewModel.ContinuationCommand.ExecuteAsync(null);
        }

        private async void New_Clicked(object? sender, EventArgs e)
        {
            if (ViewModel is null || OverlayViewModel is null)
                return;

            ViewModel.CreationType = NewVaultCreationType.CreateNew;
            await OverlayViewModel.ContinuationCommand.ExecuteAsync(null);
        }
    }
}
