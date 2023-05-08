using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Core;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Banners;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.UserControls.InfoBars;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.Views.Settings
{
    internal sealed partial class PreferencesSettingsPage : Page
    {
        /// <summary>
        /// Whether to play the InfoBar show animation after its layout is updated.
        /// </summary>
        private bool _playShowFileSystemInfoBarAnimation;

        /// <summary>
        /// Whether the adapter status has been updated at least once since navigation.
        /// </summary>
        private bool _adapterStatusUpdated;

        public PreferencesSettingsViewModel? ViewModel
        {
            get => (PreferencesSettingsViewModel?)DataContext;
            set => DataContext = value;
        }

        public PreferencesSettingsPage()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is PreferencesSettingsViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        public override void OnNavigatingFrom()
        {
            if (IsInfoBarOpen)
                QuickHideFileSystemInfoBarStoryboard.RunAnimationsAsync();
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
            var fileSystemAdapterResult = await fileSystemAdapter.GetStatusAsync(cancellationToken);

            if (!fileSystemAdapterResult.Successful)
            {
                newFileSystemInfoBar = fileSystemAdapter.Id switch
                {
                    Constants.FileSystemId.DOKAN_ID => new DokanyInfoBar(),
                    Constants.FileSystemId.FUSE_ID => new FuseInfoBar(),
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
            else if (fileSystemAdapter.Id == Constants.FileSystemId.WEBDAV_ID)
            {
                newFileSystemInfoBar = new WebDavInfoBar();
                newFileSystemInfoBar.IsOpen = true;
                newFileSystemInfoBar.InfoBarSeverity = InfoBarSeverityType.Warning;
                newFileSystemInfoBar.CanBeClosed = false;
            }
            else if (fileSystemAdapterResult.Successful && newFileSystemInfoBar is not null)
            {
                newFileSystemInfoBar.IsOpen = false;
            }

            var wasOpen = IsInfoBarOpen;
            var isOpen = newFileSystemInfoBar?.IsOpen ?? false;

            if (wasOpen)
                await HideFileSystemInfoBarStoryboard.RunAnimationsAsync();

            if (!_adapterStatusUpdated)
            {
                await Task.Delay(500);
                _adapterStatusUpdated = true;
            }

            FileSystemInfoBar = newFileSystemInfoBar;
            IsInfoBarOpen = _playShowFileSystemInfoBarAnimation = isOpen;
        }

        private async Task<FileSystemAdapterItemViewModel?> GetSupportedAdapter(CancellationToken cancellationToken = default)
        {
            foreach (var item in ViewModel.BannerViewModel.FileSystemAdapters)
            {
                var isSupportedResult = await item.FileSystemInfoModel.GetStatusAsync(cancellationToken);
                if (isSupportedResult.Successful)
                    return item;
            }

            return ViewModel.BannerViewModel.FileSystemAdapters.FirstOrDefault();
        }

        private void FileSystemInfoBarContainer_LayoutUpdated(object? sender, EventArgs e)
        {
            if (_playShowFileSystemInfoBarAnimation)
            {
                ShowFileSystemInfoBarStoryboard.RunAnimationsAsync();
                _playShowFileSystemInfoBarAnimation = false;
            }
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

        private void FileSystemAdapterChoice_Loaded(object? sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
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