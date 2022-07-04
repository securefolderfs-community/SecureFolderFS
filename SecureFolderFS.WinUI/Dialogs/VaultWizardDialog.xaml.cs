using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.UI.Animations;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;
using SecureFolderFS.WinUI.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Dialogs
{
    public sealed partial class VaultWizardDialog : ContentDialog, IDialog<VaultWizardDialogViewModel>, IRecipient<NavigationRequestedMessage>, IRecipient<BackNavigationRequestedMessage>
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

        /// <inheritdoc/>
        public async void Receive(NavigationRequestedMessage message)
        {
            await FinalizeNavigationAnimationAsync((message.ViewModel as BaseVaultWizardPageViewModel)!);
        }

        /// <inheritdoc/>
        public async void Receive(BackNavigationRequestedMessage message)
        {
            await FinalizeNavigationAnimationAsync(ViewModel.CurrentPageViewModel!);
        }

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
            }
            else if (!_isBackAnimationState && Navigation.CanGoBack)
            {
                _isBackAnimationState = true;
                GoBack.Visibility = Visibility.Visible;
                await ShowBackButtonStoryboard.BeginAsync();
                ShowBackButtonStoryboard.Stop();
            }
            else if (_isBackAnimationState && !Navigation.CanGoBack)
            {
                _isBackAnimationState = false;
                await HideBackButtonStoryboard.BeginAsync();
                HideBackButtonStoryboard.Stop();
                GoBack.Visibility = Visibility.Collapsed;
            }

            GoBack.Visibility = Navigation.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void VaultWizardDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // Register order is important!
            ViewModel.Messenger.Register<NavigationRequestedMessage>(Navigation);
            ViewModel.Messenger.Register<BackNavigationRequestedMessage>(Navigation);
            ViewModel.Messenger.Register<NavigationRequestedMessage>(this);
            ViewModel.Messenger.Register<BackNavigationRequestedMessage>(this);

            var viewModel = new MainVaultWizardPageViewModel(ViewModel.Messenger, ViewModel);
            Navigation.Navigate(viewModel, new SuppressNavigationTransitionInfo());
            await FinalizeNavigationAnimationAsync(viewModel);
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

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (!args.Cancel)
                Navigation.Dispose();
        }
    }
}
