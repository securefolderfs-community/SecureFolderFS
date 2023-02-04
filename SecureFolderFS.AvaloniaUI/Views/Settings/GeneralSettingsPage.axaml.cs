using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.Helpers;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Pages.Settings;
using SecureFolderFS.UI.Enums;

namespace SecureFolderFS.AvaloniaUI.Views.Settings
{
    internal sealed partial class GeneralSettingsPage : Page
    {
        /// <summary>
        /// Whether to play the InfoBar show animation after its layout is updated.
        /// </summary>
        private bool _playShowVersionInfoBarAnimation;

        public GeneralSettingsPageViewModel ViewModel
        {
            get => (GeneralSettingsPageViewModel)DataContext;
            set => DataContext = value;
        }

        public int SelectedThemeIndex
        {
            get => (int)ThemeHelper.Instance.CurrentTheme;
            set => ThemeHelper.Instance.CurrentTheme = (ApplicationTheme)value;
        }

        public GeneralSettingsPage()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is GeneralSettingsPageViewModel viewModel)
                ViewModel = viewModel;
        }

        /// <inheritdoc/>
        public override void OnNavigatingFrom()
        {
            QuickHideVersionInfoBarStoryboard.RunAnimationsAsync();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Control_OnLoaded(object? sender, RoutedEventArgs e)
        {
            _ = ViewModel.BannerViewModel.ConfigureUpdates();
        }

        private void RootGrid_OnLoaded(object? sender, RoutedEventArgs e)
        {
            _ = AddItemsTransitionAsync();
        }

        private void ComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox { SelectedItem: ILanguageModel language })
                ViewModel.LanguageSettingViewModel.UpdateCurrentLanguage(language);
        }

        private async Task AddItemsTransitionAsync()
        {
            // TODO Transition
            // Await a short delay for page navigation transition to complete and set ReorderThemeTransition to animate items when layout changes.
            // await Task.Delay(400);
            // RootGrid?.Transitions?.Add(new ReorderThemeTransition());
        }

        // TODO Replace this workaround with something better
        private void ComboBox_OnLoaded(object? sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
                comboBox.SelectedIndex = 0; // TODO Remove this when fixing languages
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