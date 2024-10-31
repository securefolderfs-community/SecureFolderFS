using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Enums;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Views.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [INotifyPropertyChanged]
    public sealed partial class PreferencesSettingsPage : Page
    {
        public PreferencesSettingsViewModel? ViewModel
        {
            get => DataContext.TryCast<PreferencesSettingsViewModel>();
            set { DataContext = value; OnPropertyChanged(); }
        }

        public PreferencesSettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is PreferencesSettingsViewModel viewModel)
            {
                ViewModel = viewModel;
                ViewModel.BannerViewModel.PropertyChanged += BannerViewModel_PropertyChanged;
            }

            _ = UpdateAdapterStatus();
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (ViewModel is not null)
                ViewModel.BannerViewModel.PropertyChanged -= BannerViewModel_PropertyChanged;

            base.OnNavigatingFrom(e);
        }

        private async void BannerViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.BannerViewModel.SelectedItem))
                await UpdateAdapterStatus();
        }

        private async Task UpdateAdapterStatus(CancellationToken cancellationToken = default)
        {
            if (ViewModel is null)
                return;

            var fileSystem = ViewModel.BannerViewModel.SelectedItem?.FileSystem;
            if (fileSystem is null)
                return;

            if (fileSystem.Id == Core.WebDav.Constants.FileSystem.FS_ID)
            {
                ViewModel.BannerViewModel.FileSystemInfoBar.IsOpen = true;
                ViewModel.BannerViewModel.FileSystemInfoBar.IsCloseable = false;
                ViewModel.BannerViewModel.FileSystemInfoBar.Severity = SeverityType.Warning;
                ViewModel.BannerViewModel.FileSystemInfoBar.Message = "WebDav is experimental. You may encounter bugs and stability issues. We recommend backing up your data before using WebDav.";
            }
            else
            {
                var fileSystemResult = await fileSystem.GetStatusAsync(cancellationToken);
                if (fileSystemResult == FileSystemAvailability.Available)
                {
                    ViewModel.BannerViewModel.FileSystemInfoBar.IsOpen = false;
                    return;
                }

                ViewModel.BannerViewModel.FileSystemInfoBar.IsOpen = true;
                ViewModel.BannerViewModel.FileSystemInfoBar.IsCloseable = false;
                ViewModel.BannerViewModel.FileSystemInfoBar.Severity = SeverityType.Critical;
                ViewModel.BannerViewModel.FileSystemInfoBar.Message = fileSystemResult switch
                {
                    FileSystemAvailability.ModuleUnavailable or FileSystemAvailability.CoreUnavailable => "Dokany has not been detected. Please install Dokany (v2.0.5) to continue using SecureFolderFS.",
                    FileSystemAvailability.VersionTooLow => "The installed version of Dokany is outdated. Please update Dokany to the match requested version. (v2.0.5)",
                    FileSystemAvailability.VersionTooHigh => "The installed version of Dokany is not compatible with SecureFolderFS version. Please install requested version of Dokany. (v2.0.5)",
                    _ => "SecureFolderFS cannot work with this version. Please install the required version of Dokany. (v2.0.5)"
                };
            }
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            _ = AddTransitionsAsync();
            return;

            async Task AddTransitionsAsync()
            {
                // Await a short delay for page navigation transition to complete and set ReorderThemeTransition to animate items when layout changes.
                await Task.Delay(400);
                (sender as Panel)?.ChildrenTransitions?.Add(new ReorderThemeTransition());
            }
        }
    }
}
