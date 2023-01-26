using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Messaging;
using ExCSS;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using Microsoft.AspNetCore.Components;
using SecureFolderFS.AvaloniaUI.UserControls.Navigation;
using SecureFolderFS.AvaloniaUI.WindowViews;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard.ExistingVault;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard.NewVault;
using SecureFolderFS.WinUI.Helpers;
using Dispatcher = Avalonia.Threading.Dispatcher;

namespace SecureFolderFS.AvaloniaUI.Dialogs
{
    public partial class VaultWizardDialog : ContentDialog, IDialog<VaultWizardDialogViewModel>, IRecipient<NavigationRequestedMessage>, IRecipient<BackNavigationRequestedMessage>, IStyleable
    {
        private bool _hasNavigationAnimatedOnLoaded;
        private bool _isBackAnimationState;

        /// <inheritdoc/>
        public VaultWizardDialogViewModel ViewModel
        {
            get => (VaultWizardDialogViewModel)DataContext;
            set => DataContext = value;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public Type StyleKey => typeof(ContentDialog);

        /// <inheritdoc/>
        public async Task<DialogResult> ShowAsync()
        {
            // Can't be in the constructor because ViewModel is set later
            InitializeComponent();
            return (DialogResult)await base.ShowAsync();
        }

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
            var canGoBack = false;

            switch (viewModel)
            {
                case MainVaultWizardPageViewModel:
                    TitleText.Text = "Add new vault";
                    PrimaryButtonText = string.Empty;
                    break;

                case VaultWizardSelectLocationViewModel:
                    TitleText.Text = "Add existing vault";
                    PrimaryButtonText = "Continue";
                    canGoBack = true;
                    break;

                case VaultWizardCreationPathViewModel:
                    TitleText.Text = "Create new vault";
                    PrimaryButtonText = "Continue";
                    canGoBack = true;
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
                GoBack.IsVisible = false;
            }
            else if (!_isBackAnimationState && (canGoBack && Navigation.CanGoBack))
            {
                _isBackAnimationState = true;
                GoBack.IsVisible = true;
                //await ShowBackButtonStoryboard.BeginAsync();
                //ShowBackButtonStoryboard.Stop();
            }
            else if (_isBackAnimationState && !(canGoBack && Navigation.CanGoBack))
            {
                _isBackAnimationState = false;
                //await HideBackButtonStoryboard.BeginAsync();
                //HideBackButtonStoryboard.Stop();
                GoBack.IsVisible = false;
            }

            GoBack.IsVisible = canGoBack && Navigation.CanGoBack;
        }

        private void VaultWizardDialog_OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (!args.Cancel)
                Navigation.Dispose();
        }

        private void VaultWizardDialog_OnLoaded(object? sender, RoutedEventArgs e)
        {
            // Register order is important!
            ViewModel.Messenger.Register<NavigationRequestedMessage>(Navigation);
            ViewModel.Messenger.Register<BackNavigationRequestedMessage>(Navigation);
            ViewModel.Messenger.Register<NavigationRequestedMessage>(this);
            ViewModel.Messenger.Register<BackNavigationRequestedMessage>(this);

            var viewModel = new MainVaultWizardPageViewModel(ViewModel.Messenger, ViewModel);
            Navigation.Navigate(viewModel, new SuppressNavigationTransitionInfo());
            _ = FinalizeNavigationAnimationAsync(viewModel);
        }

        private void VaultWizardDialog_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var eventDispatchFlag = new EventDispatchFlagHelper(() => args.Cancel = true);
            ViewModel.PrimaryButtonClickCommand?.Execute(eventDispatchFlag);
        }

        private void VaultWizardDialog_OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var eventDispatchFlag = new EventDispatchFlagHelper(() => args.Cancel = true);
            ViewModel.SecondaryButtonClickCommand?.Execute(eventDispatchFlag);
        }
    }
}