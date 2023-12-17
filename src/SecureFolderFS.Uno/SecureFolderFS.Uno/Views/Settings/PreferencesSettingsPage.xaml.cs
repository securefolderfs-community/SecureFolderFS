using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Banners;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.UserControls.InfoBars;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Views.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PreferencesSettingsPage : Page
    {
        public PreferencesSettingsViewModel ViewModel
        {
            get => (PreferencesSettingsViewModel)DataContext;
            set => DataContext = value;
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
            var fileSystemAdapter = ViewModel.BannerViewModel.SelectedItem?.FileSystemInfoModel;
            if (fileSystemAdapter is null)
                return;

            var adapterResult = await fileSystemAdapter.GetStatusAsync(cancellationToken);
            if (fileSystemAdapter.Id == Core.Constants.FileSystemId.WEBDAV_ID)
            {
                FileSystemInfoBar = new WebDavInfoBar();
                FileSystemInfoBar.IsOpen = true;
                FileSystemInfoBar.Severity = InfoBarSeverityType.Warning;
                FileSystemInfoBar.CanBeClosed = false;
            }
            else if (adapterResult.Successful && FileSystemInfoBar is not null)
            {
                FileSystemInfoBar.IsOpen = false;
            }
            else if (!adapterResult.Successful)
            {
                FileSystemInfoBar = fileSystemAdapter.Id switch
                {
                    Core.Constants.FileSystemId.DOKAN_ID => new DokanyInfoBar(),
                    _ => null
                };
                if (FileSystemInfoBar is null)
                    return;

                await Task.Delay(800, cancellationToken);
                FileSystemInfoBar.IsOpen = true;
                FileSystemInfoBar.Severity = InfoBarSeverityType.Error;
                FileSystemInfoBar.CanBeClosed = false;
                FileSystemInfoBar.Message = adapterResult.GetMessage("Invalid state.");
            }

            IsInfoBarOpen = FileSystemInfoBar?.IsOpen ?? false;
        }

        private async Task<FileSystemItemViewModel?> GetSupportedAdapter(CancellationToken cancellationToken = default)
        {
            foreach (var item in ViewModel.BannerViewModel.FileSystemAdapters)
            {
                var isSupportedResult = await item.FileSystemInfoModel.GetStatusAsync(cancellationToken);
                if (isSupportedResult.Successful)
                    return item;
            }

            return ViewModel.BannerViewModel.FileSystemAdapters.FirstOrDefault();
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            _ = AddTransitionsAsync();
            async Task AddTransitionsAsync()
            {
                // Await a short delay for page navigation transition to complete and set ReorderThemeTransition to animate items when layout changes.
                await Task.Delay(400);
                (sender as Panel)?.ChildrenTransitions?.Add(new ReorderThemeTransition());
            }
        }

        public InfoBarViewModel FileSystemInfoBar
        {
            get => (InfoBarViewModel)GetValue(FileSystemInfoBarProperty);
            set => SetValue(FileSystemInfoBarProperty, value);
        }
        public static readonly DependencyProperty FileSystemInfoBarProperty =
            DependencyProperty.Register(nameof(FileSystemInfoBar), typeof(InfoBarViewModel), typeof(PreferencesSettingsPage), new PropertyMetadata(defaultValue: null));

        public bool IsInfoBarOpen
        {
            get => (bool)GetValue(IsInfoBarOpenProperty);
            set => SetValue(IsInfoBarOpenProperty, value);
        }
        public static readonly DependencyProperty IsInfoBarOpenProperty =
            DependencyProperty.Register(nameof(IsInfoBarOpen), typeof(bool), typeof(PreferencesSettingsPage), new PropertyMetadata(defaultValue: false));
    }
}
