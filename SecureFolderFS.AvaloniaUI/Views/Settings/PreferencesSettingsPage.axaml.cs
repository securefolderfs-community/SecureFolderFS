using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using SecureFolderFS.AvaloniaUI.Animations;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Pages.Settings;
using SecureFolderFS.Sdk.ViewModels.Settings.Banners;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.UserControls.InfoBars;

namespace SecureFolderFS.AvaloniaUI.Views.Settings
{
    internal sealed partial class PreferencesSettingsPage : Page
    {
        private bool _hasPlayedFileSystemInfoBarAnimation;

        public PreferencesSettingsPageViewModel ViewModel
        {
            get => (PreferencesSettingsPageViewModel)DataContext;
            set => DataContext = value;
        }

        public PreferencesSettingsPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is PreferencesSettingsPageViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        public override void OnNavigatingFrom()
        {
            if (IsInfoBarOpen)
                QuickHideFileSystemInfoBarStoryboard.RunAnimationsAsync();
        }

        private async void PreferencesSettingsPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.BannerViewModel.InitAsync();

            FileSystemAdapterChoice.SelectedItem = ViewModel.BannerViewModel.FileSystemAdapters
                .FirstOrDefault(x =>
                    x.FileSystemInfoModel.Id.Equals(ViewModel.BannerViewModel.PreferredFileSystemId));

            FileSystemAdapterChoice.SelectedItem ??= await GetSupportedAdapter();
        }

        private async void FileSystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileSystemAdapterChoice.SelectedIndex == -1 || FileSystemAdapterChoice.SelectedIndex > ViewModel.BannerViewModel.FileSystemAdapters.Count)
                return; // Fix crash upon changing page

            ViewModel.BannerViewModel.PreferredFileSystemId = ViewModel.BannerViewModel.FileSystemAdapters[FileSystemAdapterChoice.SelectedIndex].FileSystemInfoModel.Id;
            await UpdateAdapterStatus((FileSystemAdapterChoice.SelectedItem as FileSystemAdapterItemViewModel)?.FileSystemInfoModel);
        }

        private async Task UpdateAdapterStatus(IFileSystemInfoModel? fileSystemAdapter, CancellationToken cancellationToken = default)
        {
            if (fileSystemAdapter is null)
                return;

            var newFileSystemInfoBar = FileSystemInfoBar;
            var fileSystemAdapterResult = await fileSystemAdapter.IsSupportedAsync(cancellationToken);
            if (fileSystemAdapter.Id == Core.Constants.FileSystemId.WEBDAV_ID)
            {
                newFileSystemInfoBar = new WebDavInfoBar();
                newFileSystemInfoBar.IsOpen = true;
                newFileSystemInfoBar.InfoBarSeverity = InfoBarSeverityType.Warning;
                newFileSystemInfoBar.CanBeClosed = false;
            }
            else if (fileSystemAdapter.Id == Core.Constants.FileSystemId.FUSE_ID)
            {
                newFileSystemInfoBar = new FuseInfoBar();
                newFileSystemInfoBar.IsOpen = true;
                newFileSystemInfoBar.InfoBarSeverity = InfoBarSeverityType.Warning;
                newFileSystemInfoBar.CanBeClosed = false;
            }
            else if (fileSystemAdapterResult.Successful && newFileSystemInfoBar is not null)
            {
                newFileSystemInfoBar.IsOpen = false;
            }
            else if (!fileSystemAdapterResult.Successful)
            {
                newFileSystemInfoBar = fileSystemAdapter.Id switch
                {
                    Core.Constants.FileSystemId.DOKAN_ID => new DokanyInfoBar(),
                    Core.Constants.FileSystemId.FUSE_ID => new FuseInfoBar(),
                    _ => null
                };
                if (newFileSystemInfoBar is null)
                    return;

                await Task.Delay(800, cancellationToken);
                newFileSystemInfoBar.IsOpen = true;
                newFileSystemInfoBar.InfoBarSeverity = InfoBarSeverityType.Error;
                newFileSystemInfoBar.CanBeClosed = false;
                newFileSystemInfoBar.Message = fileSystemAdapterResult.GetMessage("Invalid state.");
            }

            var wasOpen = IsInfoBarOpen;
            var isOpen = newFileSystemInfoBar?.IsOpen ?? false;

            if (wasOpen)
                await HideFileSystemInfoBarStoryboard.RunAnimationsAsync();

            FileSystemInfoBar = newFileSystemInfoBar;
            IsInfoBarOpen = isOpen;

            if (isOpen)
            {
                if (!_hasPlayedFileSystemInfoBarAnimation)
                {
                    _hasPlayedFileSystemInfoBarAnimation = true;
                    await Task.Delay(50); // Wait until layout is loaded (TODO do it properly)
                    ((TranslateTransform)FileSystemInfoBarContainer.RenderTransform!).Y = -FileSystemInfoBarContainer.Bounds.Height;
                    ((TranslateTransform)OtherSettings.RenderTransform!).Y = -FileSystemInfoBarContainer.Bounds.Height;
                    await Task.Delay(500);
                }

                await ShowFileSystemInfoBarStoryboard.RunAnimationsAsync();
            }

            _hasPlayedFileSystemInfoBarAnimation = true;
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
            // TODO Transition
            // Await a short delay for page navigation transition to complete and set ReorderThemeTransition to animate items when layout changes.
            // await Task.Delay(400);
            // RootGrid?.ChildrenTransitions?.Add(new ReorderThemeTransition());
        }

        private void FileSystemAdapterChoice_OnLoaded(object? sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        public InfoBarViewModel? FileSystemInfoBar
        {
            get => GetValue(FileSystemInfoBarProperty);
            set => SetValue(FileSystemInfoBarProperty, value);
        }
        public static readonly StyledProperty<InfoBarViewModel?> FileSystemInfoBarProperty =
            AvaloniaProperty.Register<PreferencesSettingsPage, InfoBarViewModel?>(nameof(FileSystemInfoBar));

        public bool IsInfoBarOpen
        {
            get => GetValue(IsInfoBarOpenProperty);
            set => SetValue(IsInfoBarOpenProperty, value);
        }
        public static readonly StyledProperty<bool> IsInfoBarOpenProperty =
            AvaloniaProperty.Register<PreferencesSettingsPage, bool>(nameof(IsInfoBarOpen));
    }
}