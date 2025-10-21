using System.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Maui.Views.Vault;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.EventArguments;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
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
        private readonly List<IViewable> _views;
        private readonly INavigation _sourceNavigation;
        private readonly TaskCompletionSource<IResult> _modalTcs;
        private IStagingView? _previousView;

        public MainWizardViewModel? ViewModel { get; private set; }

        public WizardOverlayViewModel? OverlayViewModel { get; private set; }

        public MainWizardPage(INavigation sourceNavigation)
        {
            _views = new();
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
            var navigationPage = new NavigationPage(this);
#if ANDROID
            navigationPage.BackgroundColor = Color.FromArgb("#80000000");
            await _sourceNavigation.PushModalAsync(navigationPage);
#elif IOS
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
            OverlayViewModel.PropertyChanged += OverlayViewModel_PropertyChanged;
            OverlayViewModel.CurrentViewModel = ViewModel;
            Shell.Current.Navigated += Shell_Navigated;

            OnPropertyChanged(nameof(ViewModel));
            OnPropertyChanged(nameof(OverlayViewModel));
        }

        private void OverlayViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(OverlayViewModel.CurrentViewModel))
                return;

            if (OverlayViewModel?.CurrentViewModel is { } viewable)
                _views.Add(viewable);
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

        private async Task<IVaultModel?> FromDataSourceAsync(BaseDataSourceWizardViewModel dataSource)
        {
            var folder = await dataSource.GetFolderAsync();
            if (folder is null)
                return null;

            var storageSource = dataSource.ToStorageSource();
            if (storageSource is null)
                return null;

            var dataModel = new VaultDataModel(folder.GetPersistableId(), folder.Name, null, storageSource);
            return dataSource switch
            {
                PickerSourceWizardViewModel => new VaultModel(folder, dataModel),
                AccountSourceWizardViewModel { SelectedAccount: { } accountViewModel } => new VaultModel(accountViewModel, dataModel, folder),
                _ => null
            };
        }

        private void Shell_Navigated(object? sender, ShellNavigatedEventArgs e)
        {
            if (e.Current.Location.OriginalString.Contains("NavigationPage"))
                return;

            Shell.Current.Navigated -= Shell_Navigated;
            if (OverlayViewModel is not null)
            {
                OverlayViewModel.NavigationRequested -= ViewModel_NavigationRequested;
                OverlayViewModel.PropertyChanged -= OverlayViewModel_PropertyChanged;

                _views.DisposeAll();
                _views.Clear();
            }

            _modalTcs.TrySetResult(OverlayViewModel?.CurrentViewModel is SummaryWizardViewModel
                ? Result.Success
                : Result.Failure(null));
        }

        private async void ViewModel_NavigationRequested(object? sender, NavigationRequestedEventArgs e)
        {
            if (OverlayViewModel is null)
                return;

            IViewable? nextViewModel = null;
            switch (e)
            {
                case DismissNavigationRequestedEventArgs:
                {
                    await HideAsync();
                    return;
                }

                case DestinationNavigationRequestedEventArgs args:
                {
                    nextViewModel = args.Destination;
                    break;
                }

                case BackNavigationRequestedEventArgs:
                {
                    // Swap views
                    (_previousView, OverlayViewModel.CurrentViewModel) = (OverlayViewModel.CurrentViewModel, _previousView);

                    // Navigate back
                    await Navigation.PopAsync(true);
                    return;
                }

                default:
                {
                    switch (e.Origin)
                    {
                        // Main -> Source Selection
                        case MainWizardViewModel viewModel:
                        {
                            nextViewModel = new SourceSelectionWizardViewModel(viewModel.Mode, OverlayViewModel, OverlayViewModel.VaultCollectionModel);
                            break;
                        }

                        // Source Selection -> Data Source
                        case SourceSelectionWizardViewModel viewModel:
                        {
                            nextViewModel = viewModel.SelectedSource;
                            if (viewModel.SelectedSource is INavigatable navigatable)
                            {
                                navigatable.NavigationRequested -= ViewModel_NavigationRequested;
                                navigatable.NavigationRequested += ViewModel_NavigationRequested;
                            }

                            break;
                        }

                        // Data Source -> Summary
                        case BaseDataSourceWizardViewModel { Mode: NewVaultMode.AddExisting } viewModel:
                        {
                            var vaultModel = await FromDataSourceAsync(viewModel);
                            if (vaultModel is null)
                                break;

                            nextViewModel = new SummaryWizardViewModel(vaultModel, OverlayViewModel.VaultCollectionModel);
                            break;
                        }

                        // Data Source -> Credentials Selection
                        case BaseDataSourceWizardViewModel { Mode: NewVaultMode.CreateNew } viewModel:
                        {
                            var vaultModel = await FromDataSourceAsync(viewModel);
                            if (vaultModel is null)
                                break;

                            nextViewModel = new CredentialsWizardViewModel(vaultModel);
                            break;
                        }

                        // Credentials Selection -> Recovery
                        case CredentialsWizardViewModel viewModel:
                        {
                            if (e is not WizardNavigationRequestedEventArgs { Result: CredentialsResult credentialsResult })
                                break;

                            nextViewModel = new RecoveryWizardViewModel(viewModel.VaultModel, credentialsResult);
                            break;
                        }

                        // Recovery -> Summary
                        case RecoveryWizardViewModel viewModel:
                        {
                            nextViewModel = new SummaryWizardViewModel(viewModel.VaultModel, OverlayViewModel.VaultCollectionModel);
                            break;
                        }

                        // Account Creation -> Go Back
                        case AccountCreationWizardViewModel _:
                        {
                            // Swap views
                            (_previousView, OverlayViewModel.CurrentViewModel) = (OverlayViewModel.CurrentViewModel, _previousView);

                            // Navigate back
                            await Navigation.PopAsync(true);
                            return;
                        }
                    }
                    break;
                }
            }

            if (nextViewModel is null)
            {
                _modalTcs.SetResult(e.Origin is SummaryWizardViewModel ? Result.Success : Result.Failure(null));
                await HideAsync();
                return;
            }

            var page = (ContentPage?)(nextViewModel switch
            {
                BrowserViewModel viewModel => new BrowserPage(viewModel),
                MainWizardViewModel => new MainWizardPage(_sourceNavigation),
                SourceSelectionWizardViewModel viewModel => new SourceSelectionWizardPage(viewModel, OverlayViewModel),
                AccountCreationWizardViewModel viewModel => new AccountCreationWizardPage(viewModel, OverlayViewModel),
                AccountSourceWizardViewModel viewModel => new AccountListSourceWizardPage(viewModel, OverlayViewModel),
                PickerSourceWizardViewModel viewModel => new PickerSourceWizardPage(viewModel, OverlayViewModel),
                CredentialsWizardViewModel viewModel => new CredentialsWizardPage(viewModel, OverlayViewModel),
                RecoveryWizardViewModel viewModel => new RecoveryWizardPage(viewModel, OverlayViewModel),
                SummaryWizardViewModel viewModel => new SummaryWizardPage(viewModel, OverlayViewModel),
                _ => null
            });

            if (page is null)
                return;

            _previousView = OverlayViewModel.CurrentViewModel;
            OverlayViewModel.CurrentViewModel = nextViewModel as IStagingView;
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
