using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Banners;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.UserControls.InfoBars;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views.Settings
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

        //public InfoBarViewModel? FileSystemInfoBar { get; private set; }

        //public bool IsInfoBarOpen => FileSystemInfoBar?.IsOpen ?? false;

        public PreferencesSettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is PreferencesSettingsViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        private async void PreferencesSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.BannerViewModel.InitAsync();

            FileSystemAdapterChoice.SelectedItem = ViewModel.BannerViewModel.FileSystemAdapters
                .FirstOrDefault(x =>
                    x.FileSystemInfoModel.Id.Equals(ViewModel.BannerViewModel.PreferredFileSystemId));

            FileSystemAdapterChoice.SelectedItem ??= await GetSupportedAdapter();
        }

        private async void FileSystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.BannerViewModel.PreferredFileSystemId = ViewModel.BannerViewModel.FileSystemAdapters[FileSystemAdapterChoice.SelectedIndex].FileSystemInfoModel.Id;
            await UpdateAdapterStatus((FileSystemAdapterChoice.SelectedItem as FileSystemAdapterItemViewModel)?.FileSystemInfoModel);
        }

        private async Task UpdateAdapterStatus(IFileSystemInfoModel? fileSystemAdapter, CancellationToken cancellationToken = default)
        {
            if (fileSystemAdapter is null)
                return;

            var fileSystemAdapterResult = await fileSystemAdapter.IsSupportedAsync(cancellationToken);
            if (fileSystemAdapter.Id == Core.Constants.FileSystemId.WEBDAV_ID)
            {
                FileSystemInfoBar = new WebDavInfoBar();
                FileSystemInfoBar.IsOpen = true;
                FileSystemInfoBar.InfoBarSeverity = InfoBarSeverityType.Warning;
                FileSystemInfoBar.CanBeClosed = false;
            }
            else if (fileSystemAdapterResult.Successful && FileSystemInfoBar is not null)
            {
                FileSystemInfoBar.IsOpen = false;
            }
            else if (!fileSystemAdapterResult.Successful)
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
                FileSystemInfoBar.InfoBarSeverity = InfoBarSeverityType.Error;
                FileSystemInfoBar.CanBeClosed = false;
                FileSystemInfoBar.Message = fileSystemAdapterResult.GetMessage("Invalid state.");
            }

            IsInfoBarOpen = FileSystemInfoBar?.IsOpen ?? false;
        }

        private async Task<FileSystemAdapterItemViewModel?> GetSupportedAdapter(CancellationToken cancellationToken = default)
        {
            foreach (var item in ViewModel.BannerViewModel.FileSystemAdapters)
            {
                var isSupportedResult = await item.FileSystemInfoModel.IsSupportedAsync(cancellationToken);
                if (isSupportedResult.Successful)
                    return item;
            }

            return ViewModel.BannerViewModel.FileSystemAdapters.FirstOrDefault();
        }

        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            _ = AddItemsTransitionAsync();
        }

        private async Task AddItemsTransitionAsync()
        {
            // Await a short delay for page navigation transition to complete and set ReorderThemeTransition to animate items when layout changes.
            await Task.Delay(400);
            RootGrid?.ChildrenTransitions?.Add(new ReorderThemeTransition());
        }

        public InfoBarViewModel FileSystemInfoBar
        {
            get => (InfoBarViewModel)GetValue(FileSystemInfoBarProperty);
            set => SetValue(FileSystemInfoBarProperty, value);
        }
        public static readonly DependencyProperty FileSystemInfoBarProperty =
            DependencyProperty.Register(nameof(FileSystemInfoBar), typeof(InfoBarViewModel), typeof(PreferencesSettingsPage), new PropertyMetadata(null));

        public bool IsInfoBarOpen
        {
            get => (bool)GetValue(IsInfoBarOpenProperty);
            set => SetValue(IsInfoBarOpenProperty, value);
        }
        public static readonly DependencyProperty IsInfoBarOpenProperty =
            DependencyProperty.Register(nameof(IsInfoBarOpen), typeof(bool), typeof(PreferencesSettingsPage), new PropertyMetadata(false));
    }
}
