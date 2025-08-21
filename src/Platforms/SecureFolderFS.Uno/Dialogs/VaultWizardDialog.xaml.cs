using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.EventArguments;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.UI.Utils;
using SecureFolderFS.Uno.Extensions;
using SecureFolderFS.Uno.Views.VaultWizard;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class VaultWizardDialog : ContentDialog, IOverlayControl
    {
        private IStagingView? _previousViewModel;

        public WizardOverlayViewModel? ViewModel
        {
            get => DataContext.TryCast<WizardOverlayViewModel>();
            set => DataContext = value;
        }

        public VaultWizardDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => (await base.ShowAsync()).ParseOverlayOption();

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (WizardOverlayViewModel)viewable;
            ViewModel.OnAppearing();
        }

        /// <inheritdoc/>
        public Task HideAsync()
        {
            Hide();
            return Task.CompletedTask;
        }

        private async Task NavigateAsync(IStagingView viewModel)
        {
            if (ViewModel is null)
                return;

            _previousViewModel = ViewModel.CurrentViewModel;
            ViewModel.CurrentViewModel = viewModel;
            _ = Navigation.NavigateAsync(viewModel);

            var shouldShowBack = ViewModel.CurrentViewModel is CredentialsWizardViewModel && Navigation.ContentFrame.CanGoBack;
            await BackTitle.AnimateBackAsync(shouldShowBack);
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (ViewModel is null)
                return;

            await ViewModel.ContinuationCommand.ExecuteAsync(new EventDispatchHelper(() => args.Cancel = true));
        }

        private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (ViewModel is null)
                return;

            await ViewModel.CancellationCommand.ExecuteAsync(new EventDispatchHelper(() => args.Cancel = true));
        }

        private async void VaultWizardDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel is null)
                return;

            ViewModel.NavigationRequested += ViewModel_NavigationRequested;
            await NavigateAsync(new MainWizardViewModel(ViewModel.VaultCollectionModel));
        }

        private async void ViewModel_NavigationRequested(object? sender, NavigationRequestedEventArgs e)
        {
            if (e is DismissNavigationRequestedEventArgs)
            {
                await HideAsync();
                return;
            }


            IStagingView? nextViewModel = null;
            switch (e.Origin)
            {
                // Main -> Summary
                case MainWizardViewModel { Mode: NewVaultMode.AddExisting }:
                {
                    var viewModel = (Navigation.ContentFrame.Content as MainWizardPage)!.CurrentViewModel;
                    if (viewModel is null)
                        return;

                    var vaultModel = await FromDataSourceAsync(viewModel);
                    if (vaultModel is null)
                        return;

                    nextViewModel = new SummaryWizardViewModel(vaultModel, ViewModel!.VaultCollectionModel);
                    break;
                }

                // Main -> Credentials
                case MainWizardViewModel { Mode: NewVaultMode.CreateNew }:
                {
                    var viewModel = (Navigation.ContentFrame.Content as MainWizardPage)!.CurrentViewModel;
                    if (viewModel is null)
                        return;

                    var vaultModel = await FromDataSourceAsync(viewModel);
                    if (vaultModel is null)
                        return;

                    nextViewModel = new CredentialsWizardViewModel(vaultModel);
                    break;
                }

                // Credentials -> Recovery
                case CredentialsWizardViewModel viewModel:
                {
                    if (e is not WizardNavigationRequestedEventArgs { Result: CredentialsResult credentialsResult })
                        throw new ArgumentException(nameof(CredentialsResult));

                    nextViewModel = new RecoveryWizardViewModel(viewModel.VaultModel, credentialsResult);
                    break;
                }

                // Recovery -> Summary
                case RecoveryWizardViewModel viewModel:
                {
                    nextViewModel = new SummaryWizardViewModel(viewModel.VaultModel, ViewModel!.VaultCollectionModel);
                    break;
                }
            }

            if (nextViewModel is null)
            {
                await HideAsync();
                return;
            }

            await NavigateAsync(nextViewModel);
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

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Cancel)
                return;

            if (ViewModel is not null)
            {
                ViewModel.OnDisappearing();
                ViewModel.NavigationRequested -= ViewModel_NavigationRequested;
            }

            Navigation.Dispose();
        }

        private async void GoBack_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel is null || !Navigation.ContentFrame.CanGoBack)
                return;

            ViewModel.CurrentViewModel = _previousViewModel;
            Navigation.ContentFrame.GoBack();

            var shouldShowBack = ViewModel.CurrentViewModel is CredentialsWizardViewModel && Navigation.ContentFrame.CanGoBack;
            await BackTitle.AnimateBackAsync(shouldShowBack);
        }
    }
}
