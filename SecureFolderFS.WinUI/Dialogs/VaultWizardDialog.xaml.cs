using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.UI.Animations;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Backend.Dialogs;
using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Utils;
using SecureFolderFS.Backend.ViewModels.Dialogs;
using SecureFolderFS.Backend.ViewModels.Pages.VaultWizard;
using SecureFolderFS.WinUI.Views.VaultWizard;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

#nullable enable

namespace SecureFolderFS.WinUI.Dialogs
{
    public sealed partial class VaultWizardDialog : ContentDialog, IDialog<VaultWizardDialogViewModel>, IRecipient<VaultWizardNavigationRequestedMessage>
    {
        private bool _hasNavigationAnimatedOnLoaded;

        public VaultWizardDialogViewModel ViewModel
        {
            get => (VaultWizardDialogViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultWizardDialog()
        {
            this.InitializeComponent();
        }

        public new async Task<DialogResult> ShowAsync() => (DialogResult)await base.ShowAsync();

        public void Receive(VaultWizardNavigationRequestedMessage message)
        {
            if (message.Value is VaultWizardMainPageViewModel)
            {
                Navigate(message.Value, new SuppressNavigationTransitionInfo());
            }
            else
            {
                Navigate(message.Value, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }

        private async void Navigate(BaseVaultWizardPageViewModel viewModel, NavigationTransitionInfo transition)
        {
            switch (viewModel)
            {
                case VaultWizardMainPageViewModel:
                    ContentFrame.Navigate(typeof(VaultWizardMainPage), viewModel, transition);
                    break;

                case AddExistingVaultPageViewModel:
                    ContentFrame.Navigate(typeof(AddExistingVaultPage), viewModel, transition);
                    break;

                case ChooseVaultCreationPathPageViewModel:
                    ContentFrame.Navigate(typeof(ChooseVaultCreationPathPage), viewModel, transition);
                    break;

                case SetPasswordPageViewModel:
                    ContentFrame.Navigate(typeof(SetPasswordPage), viewModel, transition);
                    break;

                case VaultWizardFinishPageViewModel:
                    ContentFrame.Navigate(typeof(VaultWizardFinishPage), viewModel, transition);
                    break;
            }

            await FinalizeNavigationAnimationAsync(viewModel);
        }

        private async Task FinalizeNavigationAnimationAsync(BaseVaultWizardPageViewModel viewModel)
        {
            switch (viewModel)
            {
                case VaultWizardMainPageViewModel:
                    TitleText.Text = "Add new vault";
                    PrimaryButtonText = string.Empty;
                    break;

                case AddExistingVaultPageViewModel:
                    TitleText.Text = "Add existing vault";
                    PrimaryButtonText = "Continue";
                    break;

                case ChooseVaultCreationPathPageViewModel:
                    TitleText.Text = "Create new vault";
                    PrimaryButtonText = "Continue";
                    break;

                case SetPasswordPageViewModel:
                    TitleText.Text = "Set password";
                    PrimaryButtonText = "Continue";
                    break;

                case VaultWizardFinishPageViewModel:
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

            if (viewModel is not VaultWizardFinishPageViewModel && ContentFrame.CanGoBack)
            {
                GoBack.Visibility = Visibility.Visible;
                await ShowBackButtonStoryboard.BeginAsync();
                ShowBackButtonStoryboard.Stop();
            }
            else
            {
                await HideBackButtonStoryboard.BeginAsync();
                HideBackButtonStoryboard.Stop();
                GoBack.Visibility = Visibility.Collapsed;
            }
        }

        private void VaultWizardDialog_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Messenger.Register<VaultWizardNavigationRequestedMessage>(this);
            ViewModel.StartNavigation();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var handledCallback = new HandledCallback(value => args.Cancel = value);
            ViewModel.PrimaryButtonClickCommand?.Execute(handledCallback);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var handledCallback = new HandledCallback(value => args.Cancel = value);
            ViewModel.SecondaryButtonClickCommand?.Execute(handledCallback);
        }

        private async void GoBack_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.GoBack();

            if ((ContentFrame.Content as Page)?.DataContext is BaseVaultWizardPageViewModel viewModel)
            {
                viewModel.UpdateViewModelOnReturn();

                await FinalizeNavigationAnimationAsync(viewModel);
            }
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            (ContentFrame.Content as IDisposable)?.Dispose();
        }
    }
}
