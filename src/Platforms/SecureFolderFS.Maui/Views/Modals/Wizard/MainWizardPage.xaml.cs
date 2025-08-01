using OwlCore.Storage;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.EventArguments;
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
            ViewModel = new();
            OverlayViewModel = (WizardOverlayViewModel)viewable;
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

        private async void ViewModel_NavigationRequested(object? sender, NavigationRequestedEventArgs e)
        {
            if (OverlayViewModel is null)
                return;

            if (e is DismissNavigationRequestedEventArgs)
            {
                await HideAsync();
                return;
            }

            IStagingView? nextViewModel = null;
            switch (e.Origin)
            {
                // Main -> Source Selection
                case MainWizardViewModel viewModel:
                {
                    nextViewModel = new SourceSelectionWizardViewModel(viewModel.Mode, OverlayViewModel, OverlayViewModel.VaultCollectionModel);;
                    break;
                }
                
                // Source Selection -> Data Source
                case SourceSelectionWizardViewModel viewModel:
                {
                    nextViewModel = viewModel.SelectedSource;
                    break;
                }
                
                // Data Source -> Summary
                case BaseDataSourceWizardViewModel { Mode: NewVaultMode.AddExisting } viewModel:
                {
                    var folder = await viewModel.GetFolderAsync();
                    if (folder is null)
                        break;
                    
                    nextViewModel = new SummaryWizardViewModel(folder, OverlayViewModel.VaultCollectionModel);
                    break;
                }
                
                // Data Source -> Credentials Selection
                case BaseDataSourceWizardViewModel { Mode: NewVaultMode.CreateNew } viewModel:
                {
                    var folder = await viewModel.GetFolderAsync();
                    if (folder is not IModifiableFolder modifiableFolder)
                        break;

                    nextViewModel = new CredentialsWizardViewModel(modifiableFolder);
                    break;
                }
                
                // Credentials Selection -> Recovery
                case CredentialsWizardViewModel viewModel:
                {
                    if (e is not WizardNavigationRequestedEventArgs { Result: CredentialsResult credentialsResult })
                        break;
                    
                    nextViewModel = new RecoveryWizardViewModel(viewModel.Folder, credentialsResult);
                    break;
                }

                // Recovery -> Summary
                case RecoveryWizardViewModel viewModel:
                {
                    nextViewModel = new SummaryWizardViewModel(viewModel.Folder, OverlayViewModel.VaultCollectionModel);
                    break;
                }
            }

            if (nextViewModel is null)
            {
                _modalTcs.SetResult(e.Origin is SummaryWizardViewModel ? Result.Success : Result.Failure(null));
                await HideAsync();
                return;
            }

            var page = (BaseModalPage?)(nextViewModel switch
            {
                MainWizardViewModel => new MainWizardPage(_sourceNavigation),
                SourceSelectionWizardViewModel viewModel => new SourceSelectionWizardPage(viewModel, OverlayViewModel),
                PickerSourceWizardViewModel viewModel => new PickerSourceWizardPage(viewModel, OverlayViewModel),
                CredentialsWizardViewModel viewModel => new CredentialsWizardPage(viewModel, OverlayViewModel),
                RecoveryWizardViewModel viewModel => new RecoveryWizardPage(viewModel, OverlayViewModel),
                SummaryWizardViewModel viewModel => new SummaryWizardPage(viewModel, OverlayViewModel),
                _ => null
            });

            OverlayViewModel.CurrentViewModel = nextViewModel;
            await Navigation.PushAsync(page, true);
        }

        private async void Existing_Clicked(object? sender, EventArgs e)
        {
            if (ViewModel is null || OverlayViewModel is null)
                return;

            ViewModel.Mode = NewVaultMode.AddExisting;
            await OverlayViewModel.ContinuationCommand.ExecuteAsync(null);
        }

        private async void New_Clicked(object? sender, EventArgs e)
        {
            if (ViewModel is null || OverlayViewModel is null)
                return;

            ViewModel.Mode = NewVaultMode.CreateNew;
            await OverlayViewModel.ContinuationCommand.ExecuteAsync(null);
        }
    }
}
