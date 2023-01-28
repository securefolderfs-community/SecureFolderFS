using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Messaging;
using ExCSS;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using Microsoft.AspNetCore.Components;
using SecureFolderFS.AvaloniaUI.Helpers;
using SecureFolderFS.AvaloniaUI.Messages;
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

        private IEnumerable<Animations.Animation> ShowBackButtonStoryboard => new List<Animations.Animation>
        {
            new()
            {
                Duration = TimeSpan.Parse("0:0:0.2"),
                Target = GoBack,
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(OpacityProperty, 0d),
                            new Setter(IsVisibleProperty, true)
                        },
                        KeyTime = TimeSpan.Zero
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(OpacityProperty, 1d),
                            new Setter(IsVisibleProperty, true)
                        },
                        KeyTime = TimeSpan.Parse("0:0:0.2")
                    }
                }
            },
            new()
            {
                Duration = TimeSpan.Parse("0:0:0.2"),
                Easing = new CircularEaseInOut(),
                Target = TitleText,
                Children =
                {
                    new KeyFrame
                    {
                        Setters = { new Setter(TranslateTransform.XProperty, -48d) },
                        KeyTime = TimeSpan.Zero
                    },
                    new KeyFrame
                    {
                        Setters = { new Setter(TranslateTransform.XProperty, 0d) },
                        KeyTime = TimeSpan.Parse("0:0:0.2")
                    }
                }
            }
        };

        private IEnumerable<Animations.Animation> HideBackButtonStoryboard => new List<Animations.Animation>
        {
            new()
            {
                Duration = TimeSpan.Parse("0:0:0.2"),
                Target = GoBack,
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Setters = { new Setter(OpacityProperty, 1d) },
                        KeyTime = TimeSpan.Zero
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(OpacityProperty, 0d),
                            new Setter(IsVisibleProperty, false)
                        },
                        KeyTime = TimeSpan.Parse("0:0:0.2")
                    }
                }
            },
            new()
            {
                Duration = TimeSpan.Parse("0:0:0.2"),
                Easing = new CircularEaseInOut(),
                Target = TitleText,
                Children =
                {
                    new KeyFrame
                    {
                        Setters = { new Setter(TranslateTransform.XProperty, 0d) },
                        KeyTime = TimeSpan.Zero
                    },
                    new KeyFrame
                    {
                        Setters = {  new Setter(TranslateTransform.XProperty, -48d) },
                        KeyTime = TimeSpan.Parse("0:0:0.2")
                    }
                }
            }
        };

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
                    break;

                case VaultWizardSelectLocationViewModel:
                    TitleText.Text = "Add existing vault";
                    canGoBack = true;
                    break;

                case VaultWizardCreationPathViewModel:
                    TitleText.Text = "Create new vault";
                    canGoBack = true;
                    break;

                case VaultWizardPasswordViewModel:
                    TitleText.Text = "Set password";
                    break;

                case VaultWizardEncryptionViewModel:
                    TitleText.Text = "Choose encryption";
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
                await Animations.Animation.RunAsync(ShowBackButtonStoryboard);
            }
            else if (_isBackAnimationState && !(canGoBack && Navigation.CanGoBack))
            {
                _isBackAnimationState = false;
                await Animations.Animation.RunAsync(HideBackButtonStoryboard);
            }
        }

        private void VaultWizardDialog_OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (!args.Cancel)
                Navigation.Dispose();

            WeakReferenceMessenger.Default.Send(new DialogHiddenMessage());
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