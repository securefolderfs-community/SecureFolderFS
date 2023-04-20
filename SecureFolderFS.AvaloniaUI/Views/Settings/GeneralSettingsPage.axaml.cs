using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.Helpers;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using SecureFolderFS.UI.Enums;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.Views.Settings
{
    internal sealed partial class GeneralSettingsPage : Page
    {
        /// <summary>
        /// Whether to play the InfoBar show animation after its layout is updated.
        /// </summary>
        private bool _playShowVersionInfoBarAnimation;

        public GeneralSettingsViewModel? ViewModel
        {
            get => (GeneralSettingsViewModel?)DataContext;
            set => DataContext = value;
        }

        public int SelectedThemeIndex => (int)AvaloniaThemeHelper.Instance.CurrentTheme;

        public GeneralSettingsPage()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <inheritdoc/>
        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is GeneralSettingsViewModel viewModel)
                ViewModel = viewModel;
        }

        /// <inheritdoc/>
        public override void OnNavigatingFrom()
        {
            QuickHideVersionInfoBarStoryboard.RunAnimationsAsync();
        }

        private Task AddItemsTransitionAsync()
        {
            return Task.CompletedTask;
            // TODO Transition
            // Await a short delay for page navigation transition to complete and set ReorderThemeTransition to animate items when layout changes.
            // await Task.Delay(400);
            // RootGrid?.Transitions?.Add(new ReorderThemeTransition());
        }

        public Task PlayShowInfoBarAnimation(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            _playShowBarEmergeAnimation = true;
            return Task.CompletedTask;
        }

        private void RootGrid_OnLoaded(object? sender, RoutedEventArgs e)
        {
            _ = AddItemsTransitionAsync();
        }

        private async void AppLanguageComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (ViewModel is null || AppLanguageComboBox.SelectedItem is not CultureInfo cultureInfo)
                return;

            await ViewModel.LocalizationService.SetCultureAsync(cultureInfo);
        }

        private async void AppThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await AvaloniaThemeHelper.Instance.SetThemeAsync((ThemeType)AppThemeComboBox.SelectedIndex);
        }

        public void ShowVersionInfoBar(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            _playShowVersionInfoBarAnimation = true;
        }

        private void VersionInfoBar_OnLayoutUpdated(object? sender, EventArgs e)
        {
            if (_playShowVersionInfoBarAnimation)
            {
                ShowVersionInfoBarStoryboard.RunAnimationsAsync();
                _playShowVersionInfoBarAnimation = false;
            }
        }
    }
}