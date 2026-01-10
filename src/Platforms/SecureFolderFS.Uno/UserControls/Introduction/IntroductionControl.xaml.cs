using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI;
using SecureFolderFS.UI.Enums;
using SecureFolderFS.UI.Utils;
using SecureFolderFS.Uno.Extensions;
using SecureFolderFS.Uno.Helpers;
using SecureFolderFS.Uno.UserControls.InterfaceRoot;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.Introduction
{
    public sealed partial class IntroductionControl : UserControl, IOverlayControl
    {
        private Grid? _overlayContainer;
        private TitleBarControl? _customTitleBar;

        public IntroductionOverlayViewModel? ViewModel
        {
            get => DataContext.TryCast<IntroductionOverlayViewModel>();
            set => DataContext = value;
        }

        public IntroductionControl()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            if (ViewModel is null)
                return Result.Failure(null); 
            
            if (ViewModel.TaskCompletion.Task.IsCompleted)
                return ViewModel.TaskCompletion.Task.Result;
            
            if (App.Instance?.MainWindow?.Content is not MainWindowRootControl { OverlayContainer: var overlayContainer, CustomTitleBar: var customTitleBar })
                return Result.Failure(null);
            
            _customTitleBar = customTitleBar;
            _overlayContainer = overlayContainer;
            if (_overlayContainer is null)
                return Result.Failure(null);
            
            // Add this control to the overlay container
            _overlayContainer.Children.Add(this);
            await Task.Delay(300);
            
            // Set the visibility of the overlay container
            _overlayContainer.Visibility = Visibility.Visible;
            RootGrid.Opacity = 0;
            
            if (_customTitleBar is not null)
                _customTitleBar.Opacity = 0d;
            
            // Play the show animation
            await ShowOverlayStoryboard.BeginAsync();
            ShowOverlayStoryboard.Stop();
            RootGrid.Opacity = 1;
            
            // Wait for the overlay to be closed
            return await ViewModel.TaskCompletion.Task;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (IntroductionOverlayViewModel)viewable;
            if (ViewModel is { SlidesCount: < 0 })
                ViewModel.SlidesCount = 3;
        }

        /// <inheritdoc/>
        [RelayCommand]
        public async Task HideAsync()
        {
            // Play the hide animation
            await HideOverlayStoryboard.BeginAsync();
            HideOverlayStoryboard.Stop();
            
            // Hide and clean up the overlay container
            if (_overlayContainer is not null)
            {
                if (_customTitleBar is not null)
                    _customTitleBar.Opacity = 1d;
                    
                _overlayContainer.Children.Remove(this);
                _overlayContainer.Visibility = Visibility.Collapsed;
                _overlayContainer = null;
            }
            
            ViewModel?.TaskCompletion.SetResult(Result.Success);
        }
        
        private async void BackgroundWebView_Loaded(object sender, RoutedEventArgs e)
        {
            var htmlString = Constants.Introduction.BACKGROUND_WEBVIEW
                .Replace("c_bg", UnoThemeHelper.Instance.CurrentTheme switch
                {
                    ThemeType.Light => "vec3(0.80, 0.86, 0.92)",
                    _ => "vec3(0.00, 0.08, 0.15)"
                })
                .Replace("c_wave", UnoThemeHelper.Instance.CurrentTheme switch
                {
                    ThemeType.Light => "vec3(0.10, 0.42, 0.75)",
                    _ => "vec3(0.090, 0.569, 1.0)"
                });

            await BackgroundWebView.EnsureCoreWebView2Async();
            BackgroundWebView.NavigateToString(htmlString);
        }

        private void IntroductionControl_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Right:
                {
                    ViewModel?.Next();
                    e.Handled = true;

                    break;
                }

                case VirtualKey.Left:
                {
                    ViewModel?.Previous();
                    e.Handled = true;

                    break;
                }
            }
        }
    }
}
