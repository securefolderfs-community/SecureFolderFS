using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.UI.Animations;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;
using SecureFolderFS.WinUI.Helpers;
using SecureFolderFS.WinUI.Views.VaultWizard;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Dialogs
{
    public sealed partial class VaultWizardDialog : ContentDialog, IDialog<VaultWizardDialogViewModel>, IRecipient<VaultWizardNavigationRequestedMessage>
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
        public new async Task<DialogResult> ShowAsync() => (DialogResult)await base.ShowAsync();

        private async Task FinalizeNavigationAnimationAsync(BaseVaultWizardPageViewModel viewModel)
        {
            switch (viewModel)
            {
                case MainVaultWizardPageViewModel:
                    TitleText.Text = "Add new vault";
                    PrimaryButtonText = string.Empty;
                    break;

                case VaultWizardAddExistingViewModel:
                    TitleText.Text = "Add existing vault";
                    PrimaryButtonText = "Continue";
                    break;

                case VaultWizardCreationPathViewModel:
                    TitleText.Text = "Create new vault";
                    PrimaryButtonText = "Continue";
                    break;

                case VaultWizardPasswordViewModel:
                    TitleText.Text = "Set password";
                    PrimaryButtonText = "Continue";
                    break;

                case VaultWizardEncryptionViewModel:
                    TitleText.Text = "Choose encryption";
                    PrimaryButtonText = "Continue";
                    break;

                case VaultWizardSummaryViewModel:
                    TitleText.Text = "Summary";
                    PrimaryButtonText = "Close";
                    SecondaryButtonText = string.Empty;
                    break;
            }

            if (!_hasNavigationAnimatedOnLoaded)
            {
                _hasNavigationAnimatedOnLoaded = true;
                GoBack.Visibility = Visibility.Collapsed;
                return;
            }

            if (!_isBackAnimationState && viewModel.CanGoBack && ContentFrame.CanGoBack)
            {
                _isBackAnimationState = true;
                GoBack.Visibility = Visibility.Visible;
                await ShowBackButtonStoryboard.BeginAsync();
                ShowBackButtonStoryboard.Stop();
            }
            else if (_isBackAnimationState && !(viewModel.CanGoBack && ContentFrame.CanGoBack))
            {
                _isBackAnimationState = false;
                await HideBackButtonStoryboard.BeginAsync();
                HideBackButtonStoryboard.Stop();
                GoBack.Visibility = Visibility.Collapsed;
            }

            GoBack.Visibility = viewModel.CanGoBack && ContentFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
        }

        private void VaultWizardDialog_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Messenger.Register<NavigationRequestedMessage>(Navigation);
            ViewModel.Messenger.Register<BackNavigationRequestedMessage>(Navigation);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var eventDispatchFlag = new EventDispatchFlagHelper(() => args.Cancel = true);
            ViewModel.PrimaryButtonClickCommand?.Execute(eventDispatchFlag);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var eventDispatchFlag = new EventDispatchFlagHelper(() => args.Cancel = true);
            ViewModel.SecondaryButtonClickCommand?.Execute(eventDispatchFlag);
        }

        private async void GoBack_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.GoBack();

            if ((ContentFrame.Content as Page)?.DataContext is BaseVaultWizardPageViewModel viewModel)
            {
                ViewModel.CurrentPageViewModel = viewModel;
                viewModel.ReturnToViewModel();
                await FinalizeNavigationAnimationAsync(viewModel);
            }
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (!args.Cancel)
            {
                Navigation.Dispose();
            }
        }
    }
}
