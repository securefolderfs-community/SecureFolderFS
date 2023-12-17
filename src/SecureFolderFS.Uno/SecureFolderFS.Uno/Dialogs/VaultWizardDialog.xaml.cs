using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault;
using SecureFolderFS.Shared.Utilities;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.Uno.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class VaultWizardDialog : ContentDialog, IDialog<VaultWizardDialogViewModel>
    {
        private bool _hasNavigationAnimatedOnLoaded;
        private bool _isBackAnimationState;

        /// <inheritdoc/>
        public VaultWizardDialogViewModel ViewModel
        {
            get => (VaultWizardDialogViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultWizardDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => DialogExtensions.ResultFromDialogOption((DialogOption)await base.ShowAsync());

        private async Task CompleteAnimationAsync(BaseWizardPageViewModel? viewModel)
        {
            var canGoBack = false;
            switch (viewModel)
            {
                case MainWizardPageViewModel:
                    TitleText.Text = "AddNewVault".ToLocalized();
                    PrimaryButtonText = "Continue".ToLocalized();
                    break;

                case AuthCreationWizardViewModel:
                    TitleText.Text = "SetPassword".ToLocalized();
                    PrimaryButtonText = "Continue".ToLocalized();
                    canGoBack = true;
                    break;

                case RecoveryKeyWizardViewModel:
                    TitleText.Text = "VaultRecovery".ToLocalized();
                    PrimaryButtonText = "Continue".ToLocalized();
                    canGoBack = false;
                    break;

                case SummaryWizardViewModel:
                    TitleText.Text = "Summary".ToLocalized();
                    PrimaryButtonText = "Close".ToLocalized();
                    SecondaryButtonText = string.Empty;
                    canGoBack = false;
                    break;
            }

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

        private async void NavigationService_NavigationChanged(object? sender, INavigationTarget? e)
        {
            await CompleteAnimationAsync(e as BaseWizardPageViewModel);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.PrimaryButtonClickCommand.Execute(new EventDispatchHelper(() => args.Cancel = true));
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.SecondaryButtonClickCommand.Execute(new EventDispatchHelper(() => args.Cancel = true));
        }

        private async void VaultWizardDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.NavigationService.SetupNavigation(Navigation))
            {
                ViewModel.NavigationService.NavigationChanged -= NavigationService_NavigationChanged;
                ViewModel.NavigationService.NavigationChanged += NavigationService_NavigationChanged;
            }

            var viewModel = new MainWizardPageViewModel(ViewModel);
            await ViewModel.NavigationService.NavigateAsync(viewModel);
            await CompleteAnimationAsync(viewModel);
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (!args.Cancel)
            {
                ViewModel.NavigationService.NavigationChanged -= NavigationService_NavigationChanged;
                ViewModel.Dispose();
                Navigation.Dispose();
            }
        }
    }
}
