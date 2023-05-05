using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.ExistingVault;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault;
using SecureFolderFS.UI.Helpers;
using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.AvaloniaUI.Messages;

namespace SecureFolderFS.AvaloniaUI.Dialogs
{
    public partial class VaultWizardDialog : ContentDialog, IDialog<VaultWizardDialogViewModel>, IStyleable
    {
        private bool _hasNavigationAnimatedOnLoaded;
        private bool _isBackAnimationState;

        /// <inheritdoc/>
        public VaultWizardDialogViewModel? ViewModel
        {
            get => (VaultWizardDialogViewModel?)DataContext;
            set => DataContext = value;
        }

        public Type StyleKey => typeof(ContentDialog);

        /// <inheritdoc/>
        public async Task<DialogResult> ShowAsync()
        {
            // Can't be in the constructor because ViewModel is set later
            await Dispatcher.UIThread.InvokeAsync(() => AvaloniaXamlLoader.Load(this));
            return (DialogResult)await base.ShowAsync();
        }

        private async Task CompleteAnimationAsync(BaseWizardPageViewModel? viewModel)
        {
            var canGoBack = false;

            switch (viewModel)
            {
                case MainWizardPageViewModel:
                    TitleText.Text = "Add new vault";
                    break;

                case ExistingLocationWizardViewModel:
                    TitleText.Text = "Add existing vault";
                    canGoBack = true;
                    break;

                case NewLocationWizardViewModel:
                    TitleText.Text = "Create new vault";
                    canGoBack = true;
                    break;

                case PasswordWizardViewModel:
                    TitleText.Text = "Set password";
                    break;

                case EncryptionWizardViewModel:
                    TitleText.Text = "Choose encryption";
                    break;

                case SummaryWizardViewModel:
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
            else if (!_isBackAnimationState && (canGoBack && Navigation.ContentFrame.CanGoBack))
            {
                _isBackAnimationState = true;
                GoBack.IsVisible = true;
                await ShowBackButtonStoryboard.BeginAsync();
                ShowBackButtonStoryboard.Stop();
            }
            else if (_isBackAnimationState && !(canGoBack && Navigation.ContentFrame.CanGoBack))
            {
                _isBackAnimationState = false;
                await HideBackButtonStoryboard.BeginAsync();
                HideBackButtonStoryboard.Stop();
                //GoBack.IsVisible = false;
            }

            //GoBack.IsVisible = canGoBack && Navigation.CanGoBack;
        }

        private async void NavigationService_NavigationChanged(object? sender, INavigationTarget? e)
        {
            await CompleteAnimationAsync(e as BaseWizardPageViewModel);
        }

        private void VaultWizardDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.PrimaryButtonClickCommand.Execute(new EventDispatchHelper(() => args.Cancel = true));
        }

        private void VaultWizardDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.SecondaryButtonClickCommand.Execute(new EventDispatchHelper(() => args.Cancel = true));
        }

        private async void VaultWizardDialog_Loaded(object? sender, RoutedEventArgs e)
        {
            if (ViewModel is null)
                return;

            if (ViewModel.NavigationService.SetupNavigation(Navigation))
            {
                ViewModel.NavigationService.NavigationChanged -= NavigationService_NavigationChanged;
                ViewModel.NavigationService.NavigationChanged += NavigationService_NavigationChanged;
            }

            var viewModel = new MainWizardPageViewModel(ViewModel);
            await ViewModel.NavigationService.NavigateAsync(viewModel);
            await CompleteAnimationAsync(viewModel);
        }

        private void VaultWizardDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (!args.Cancel)
            {
                ViewModel.NavigationService.NavigationChanged -= NavigationService_NavigationChanged;
                ViewModel.Dispose();
                Navigation.Dispose();

                WeakReferenceMessenger.Default.Send(new DialogHiddenMessage());
            }
        }
    }
}