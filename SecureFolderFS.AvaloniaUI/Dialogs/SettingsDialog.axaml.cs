using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using SecureFolderFS.AvaloniaUI.UserControls.Navigation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using SecureFolderFS.UI.Helpers;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.Dialogs
{
    public sealed partial class SettingsDialog : ContentDialog, IDialog<SettingsDialogViewModel>, IStyleable
    {
        /// <inheritdoc/>
        public SettingsDialogViewModel ViewModel
        {
            get => (SettingsDialogViewModel)DataContext!;
            set => DataContext = value;
        }

        public Type StyleKey => typeof(ContentDialog);

        public SettingsDialog()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <inheritdoc/>
        public async Task<DialogResult> ShowAsync() => (DialogResult)await base.ShowAsync();

        private INavigationTarget GetTargetForTag(int tag)
        {
            return tag switch
            {
                0 => ViewModel.NavigationService.TryGetTarget<GeneralSettingsViewModel>() ?? new(),
                1 => ViewModel.NavigationService.TryGetTarget<PreferencesSettingsViewModel>() ?? new(),
                2 => ViewModel.NavigationService.TryGetTarget<PrivacySettingsViewModel>() ?? new(),
                3 => ViewModel.NavigationService.TryGetTarget<AboutSettingsViewModel>() ?? new(),
                _ => new GeneralSettingsViewModel()
            };
        }

        private async void NavigationView_SelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
        {
            if (!ViewModel.NavigationService.SetupNavigation(Navigation))
                return;

            var tag = Convert.ToInt32((e.SelectedItem as NavigationViewItem)?.Tag);
            var target = GetTargetForTag(tag);

            await ViewModel.NavigationService.NavigateAsync(target);
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void SettingsDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs e)
        {
            // Remove the reference to the NavigationControl so the dialog can get properly garbage collected
            ViewModel.NavigationService.ResetNavigation<FrameNavigationControl>();
        }
    }
}