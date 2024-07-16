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
        private bool _isBeingCopied;

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

        private async Task StartChangeTextAnimation(UIElement initialText, UIElement copyLabel)
        {
            try
            {
                if (_isBeingCopied)
                    return;

                _isBeingCopied = true;

                initialText.Visibility = Visibility.Collapsed;
                copyLabel.Visibility = Visibility.Visible;

                await Task.Delay(750);
                Storyboard.SetTarget(ChangeTextAnimation.Children[0], initialText);

                copyLabel.Visibility = Visibility.Collapsed;
                initialText.Visibility = Visibility.Visible;

                await ChangeTextAnimation.BeginAsync();
                ChangeTextAnimation.Stop();

                initialText.Visibility = Visibility.Visible;
                copyLabel.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                _ = ex;
            }
            finally
            {
                _isBeingCopied = false;
            }
        }

        private async void CopyAppVersion_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel is null)
                return;

            await ViewModel.CopyAppVersionCommand.ExecuteAsync(default);
            await StartChangeTextAnimation(CopyAppVersionGrid.Children[0], CopyAppVersionGrid.Children[1]);
        }

        private async void CopySystemVersion_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel is null)
                return;

            await ViewModel.CopySystemVersionCommand.ExecuteAsync(default);
            await StartChangeTextAnimation(CopySystemVersionGrid.Children[0], CopySystemVersionGrid.Children[1]);
        }
    }
}
