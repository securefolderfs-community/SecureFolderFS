using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
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

            FileSystemInfoBar = newFileSystemInfoBar;
            IsInfoBarOpen = isOpen;

            if (isOpen)
            {
                // TODO fix bouncing
                await Task.Delay(50); // Wait until layout is loaded (TODO do it properly)
                ((TranslateTransform)FileSystemInfoBarContainer.RenderTransform!).Y = -FileSystemInfoBarContainer.Bounds.Height;
                ((TranslateTransform)OtherSettings.RenderTransform!).Y = -FileSystemInfoBarContainer.Bounds.Height;

                if (!_adapterStatusUpdated)
                    await Task.Delay(500);

                await ShowFileSystemInfoBarStoryboard.RunAnimationsAsync();
            }
            else
            {
                ((TranslateTransform)FileSystemInfoBarContainer.RenderTransform!).Y = 0;
                ((TranslateTransform)OtherSettings.RenderTransform!).Y = 0;
            }

            _adapterStatusUpdated = true;
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