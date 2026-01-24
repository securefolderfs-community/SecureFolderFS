using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Uno.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Views.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [INotifyPropertyChanged]
    public sealed partial class AboutSettingsPage : Page
    {
        private bool _isAppVersionBeingCopied;
        private bool _isSystemVersionBeingCopied;

        public AboutSettingsViewModel? ViewModel
        {
            get => DataContext.TryCast<AboutSettingsViewModel>();
            set { DataContext = value; OnPropertyChanged(); }
        }

        public AboutSettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is AboutSettingsViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        private async Task StartChangeTextAnimation(UIElement initialText, UIElement copyLabel, bool isBeingCopied, Action<bool> setFlag)
        {
            try
            {
                if (isBeingCopied)
                    return;

                setFlag(true);
                initialText.Visibility = Visibility.Collapsed;
                copyLabel.Visibility = Visibility.Visible;

                await Task.Delay(750);
                
                // Create a new storyboard instance for this specific animation
                var fadeInAnimation = new DoubleAnimation()
                {
                    From = 0d,
                    To = 1d,
                    Duration = new Duration(TimeSpan.FromMilliseconds(250))
                };
                
                var storyboard = new Storyboard();
                storyboard.Children.Add(fadeInAnimation);
                Storyboard.SetTarget(fadeInAnimation, initialText);
                Storyboard.SetTargetProperty(fadeInAnimation, "Opacity");

                copyLabel.Visibility = Visibility.Collapsed;
                initialText.Visibility = Visibility.Visible;
                initialText.Opacity = 0d;

                await storyboard.BeginAsync();
                storyboard.Stop();
                initialText.Opacity = 1d;
            }
            catch (Exception ex)
            {
                _ = ex;
            }
            finally
            {
                setFlag(false);
            }
        }

        private async void CopyAppVersion_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel is null)
                return;

            await ViewModel.CopyAppVersionCommand.ExecuteAsync(default);
            await StartChangeTextAnimation(
                CopyAppVersionGrid.Children[0], 
                CopyAppVersionGrid.Children[1],
                _isAppVersionBeingCopied,
                v => _isAppVersionBeingCopied = v);
        }

        private async void CopySystemVersion_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel is null)
                return;

            await ViewModel.CopySystemVersionCommand.ExecuteAsync(default);
            await StartChangeTextAnimation(
                CopySystemVersionGrid.Children[0], 
                CopySystemVersionGrid.Children[1],
                _isSystemVersionBeingCopied,
                v => _isSystemVersionBeingCopied = v);
        }
    }
}
