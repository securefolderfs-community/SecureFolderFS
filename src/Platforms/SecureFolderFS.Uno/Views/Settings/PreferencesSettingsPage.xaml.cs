using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
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

            switch (fileSystem.Id)
            {
                case Core.WebDav.Constants.FileSystem.FS_ID:
                {
                    ViewModel.BannerViewModel.FileSystemInfoBar.IsOpen = true;
                    ViewModel.BannerViewModel.FileSystemInfoBar.IsCloseable = false;
                    ViewModel.BannerViewModel.FileSystemInfoBar.Severity = Severity.Warning;
                    ViewModel.BannerViewModel.FileSystemInfoBar.Message = "WebDav is experimental. You may encounter bugs and stability issues. We recommend backing up your data before using WebDav.";
                    break;
                }

#if WINDOWS
                case Core.Dokany.Constants.FileSystem.FS_ID:
                case Core.WinFsp.Constants.FileSystem.FS_ID:
                {
                    var fileSystemResult = await fileSystem.GetStatusAsync(cancellationToken);
                    if (fileSystemResult == FileSystemAvailability.Available)
                    {
                        ViewModel.BannerViewModel.FileSystemInfoBar.IsOpen = false;
                        return;
                    }

                    ViewModel.BannerViewModel.FileSystemInfoBar.IsOpen = true;
                    ViewModel.BannerViewModel.FileSystemInfoBar.IsCloseable = false;
                    ViewModel.BannerViewModel.FileSystemInfoBar.Severity = Severity.Critical;
                    ViewModel.BannerViewModel.FileSystemInfoBar.Message = fileSystem.Id switch
                    {
                        Core.Dokany.Constants.FileSystem.FS_ID => fileSystemResult switch
                        {
                            FileSystemAvailability.ModuleUnavailable or FileSystemAvailability.CoreUnavailable => "DokanyNotDetected".ToLocalized(Core.Dokany.Constants.FileSystem.VERSION_STRING),
                            _ => "DokanyIncompatible".ToLocalized(Core.Dokany.Constants.FileSystem.VERSION_STRING),
                        },

                        Core.WinFsp.Constants.FileSystem.FS_ID => fileSystemResult switch
                        {
                            FileSystemAvailability.ModuleUnavailable or FileSystemAvailability.CoreUnavailable => "WinFspNotDetected".ToLocalized(Core.WinFsp.Constants.FileSystem.VERSION_STRING),
                            _ => "WinFspIncompatible".ToLocalized(Core.WinFsp.Constants.FileSystem.VERSION_STRING),
                        },

                        _ => null
                    };

                    break;
                }
#endif

                default:
                    ViewModel.BannerViewModel.FileSystemInfoBar.IsOpen = false;
                    break;
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
