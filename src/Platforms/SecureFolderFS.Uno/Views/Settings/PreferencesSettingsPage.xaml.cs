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
        [ObservableProperty] private bool _IsWebDavApplyVisible;

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

            // The Apply action only accompanies the WebDav file size limit suggestion
            IsWebDavApplyVisible = false;

            switch (fileSystem.Id)
            {
                case Core.WebDav.Constants.FileSystem.FS_ID:
                {
#if WINDOWS
                    // The WebDAV redirector rejects transfers of files larger than the configured
                    // allocation limit (~47MB by default), so suggest raising it to the maximum (4GB)
                    var fileSizeLimit = Helpers.WebDavRedirectorHelpers.GetFileSizeLimit();
                    if (fileSizeLimit >= Helpers.WebDavRedirectorHelpers.MAX_FILE_SIZE_LIMIT)
                    {
                        ViewModel.BannerViewModel.FileSystemInfoBar.IsOpen = false;
                        break;
                    }

                    IsWebDavApplyVisible = true;
                    ViewModel.BannerViewModel.FileSystemInfoBar.IsOpen = true;
                    ViewModel.BannerViewModel.FileSystemInfoBar.IsCloseable = false;
                    ViewModel.BannerViewModel.FileSystemInfoBar.Severity = Severity.Default;
                    ViewModel.BannerViewModel.FileSystemInfoBar.Message = "Windows limits the size of files transferred over WebDav. Increase the allocation size to the maximum limit (4GB) to copy larger files.";
#else
                    ViewModel.BannerViewModel.FileSystemInfoBar.IsOpen = true;
                    ViewModel.BannerViewModel.FileSystemInfoBar.IsCloseable = false;
                    ViewModel.BannerViewModel.FileSystemInfoBar.Severity = Severity.Warning;
                    ViewModel.BannerViewModel.FileSystemInfoBar.Message = "WebDav is experimental. You may encounter bugs and stability issues. We recommend backing up your data before using WebDav.";
#endif
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

        private async void WebDavApply_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel is null || sender is not Button button)
                return;

            // Prevent double activation while the elevation prompt is shown
            button.IsEnabled = false;

            var applied = await Helpers.WebDavRedirectorHelpers.TrySetMaxFileSizeLimitAsync();
            button.IsEnabled = true;

            if (!applied)
                return;

            IsWebDavApplyVisible = false;
            ViewModel.BannerViewModel.FileSystemInfoBar.Severity = Severity.Success;
            ViewModel.BannerViewModel.FileSystemInfoBar.Message = "The maximum allocation size has been applied. The change will take effect after the WebClient service is restarted or Windows is rebooted.";
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
