using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.AvaloniaUI.Messages;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using SecureFolderFS.UI.Helpers;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.Dialogs
{
    internal sealed partial class SettingsDialog : ContentDialog, IDialog<SettingsDialogViewModel>, IStyleable
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

        private INotifyPropertyChanged GetTargetForTag(int tag)
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

        private async void NavigationView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
        {
            if (!ViewModel.NavigationService.SetupNavigation(Navigation))
                return;

            var tag = Convert.ToInt32((e.SelectedItem as NavigationViewItem)?.Tag);
            var target = GetTargetForTag(tag);

            await Navigation.NavigateAsync(target, new EntranceNavigationTransition());
        }

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            Hide();
            WeakReferenceMessenger.Default.Send(new DialogHiddenMessage());
        }
    }
}