using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.EventArguments;
using SecureFolderFS.Shared.Extensions;
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
        private BaseWizardViewModel? _previousViewModel;

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

        private async Task NavigateAsync(BaseWizardViewModel viewModel)
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
            if (e is CloseNavigationRequestedEventArgs)
            {
                await HideAsync();
                return;
            }

            BaseWizardViewModel? nextViewModel = e.Origin switch
            {
                // Main (if 'Add existing' selected) -> Summary
                MainWizardViewModel { CreationType: NewVaultCreationType.AddExisting } => new SummaryWizardViewModel(
                    (Navigation.ContentFrame.Content as MainWizardPage)!.CurrentViewModel!.SelectedFolder!, ViewModel!.VaultCollectionModel),

                // Main (if 'Create new' selected) -> Credentials
                MainWizardViewModel { CreationType: NewVaultCreationType.CreateNew } => new CredentialsWizardViewModel(
                    (IModifiableFolder)(Navigation.ContentFrame.Content as MainWizardPage)!.CurrentViewModel!.SelectedFolder!),

                // Credentials -> Recovery
                CredentialsWizardViewModel viewModel => new RecoveryWizardViewModel(viewModel.Folder, (e as WizardNavigationRequestedEventArgs)?.Result),

                // Recovery -> Summary
                RecoveryWizardViewModel viewModel => new SummaryWizardViewModel(viewModel.Folder, ViewModel!.VaultCollectionModel),

                // Close
                _ => null
            };

            if (nextViewModel is null)
            {
                await HideAsync();
                return;
            }

            await NavigateAsync(nextViewModel);
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
