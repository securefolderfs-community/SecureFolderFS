using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard2;
using SecureFolderFS.Shared.ComponentModel;
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
        private bool _hasNavigationAnimatedOnLoaded;
        private bool _isBackAnimationState;

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
        public new async Task<IResult> ShowAsync() => ((DialogOption)await base.ShowAsync()).ParseDialogOption();

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
            _ = Navigation.NavigateAsync(viewModel, (NavigationTransitionInfo?)null);
            await AnimateBackAsync(viewModel);
        }

        private async Task AnimateBackAsync(BaseWizardViewModel? viewModel)
        {
            var canGoBack = viewModel is CredentialsWizardViewModel;
            if (!_hasNavigationAnimatedOnLoaded)
            {
                _hasNavigationAnimatedOnLoaded = true;
                GoBack.Visibility = Visibility.Collapsed;
            }
            else switch (_isBackAnimationState)
            {
                case false when (canGoBack && Navigation.ContentFrame.CanGoBack):
                    _isBackAnimationState = true;
                    GoBack.Visibility = Visibility.Visible;
                    await ShowBackButtonStoryboard.BeginAsync();
                    ShowBackButtonStoryboard.Stop();
                    break;

                case true when !(canGoBack && Navigation.ContentFrame.CanGoBack):
                    _isBackAnimationState = false;
                    await HideBackButtonStoryboard.BeginAsync();
                    HideBackButtonStoryboard.Stop();
                    GoBack.Visibility = Visibility.Collapsed;
                    break;
            }

            GoBack.Visibility = canGoBack && Navigation.ContentFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel?.ContinuationCommand.Execute(new EventDispatchHelper(() => args.Cancel = true));
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel?.CancellationCommand.Execute(new EventDispatchHelper(() => args.Cancel = true));
        }

        private async void VaultWizardDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel is null)
                return;

            ViewModel.NavigationRequested += ViewModel_NavigationRequested;
            await NavigateAsync(new MainWizardViewModel());
        }

        private async void ViewModel_NavigationRequested(object? sender, NavigationRequestedEventArgs e)
        {
            BaseWizardViewModel nextViewModel = e.Origin switch
            {
                // Main (if existing selected) => Summary
                MainWizardViewModel { CreationType: NewVaultCreationType.AddExisting } => new SummaryWizardViewModel(
                    (Navigation.ContentFrame.Content as MainWizardPage)!.CurrentViewModel!.SelectedFolder!, ViewModel!.VaultCollectionModel),

                // Main (if new selected) => Credentials
                MainWizardViewModel { CreationType: NewVaultCreationType.CreateNew } => new CredentialsWizardViewModel((IModifiableFolder)(Navigation.ContentFrame.Content as MainWizardPage)!.CurrentViewModel!.SelectedFolder!),
                // Credentials => Recovery
                CredentialsWizardViewModel viewModel => new RecoveryWizardViewModel(viewModel.Folder, e.Result),
                // Recovery => Summary
                RecoveryWizardViewModel viewModel => new SummaryWizardViewModel(viewModel.Folder, ViewModel!.VaultCollectionModel),

                // Fallback
                _ => throw new NotImplementedException()
            };

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
            await AnimateBackAsync(ViewModel.CurrentViewModel);
        }
    }
}
