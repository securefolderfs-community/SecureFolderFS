using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions;
using SecureFolderFS.AvaloniaUI.Messages;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Pages.Settings;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
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
            get => (SettingsDialogViewModel)DataContext;
            set => DataContext = value;
        }

        public SettingsDialog()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public Type StyleKey => typeof(ContentDialog);

        /// <inheritdoc/>
        public async Task<DialogResult> ShowAsync() => (DialogResult)await base.ShowAsync();

        private INotifyPropertyChanged GetViewModelForTag(int tag)
        {
            return tag switch
            {
                0 => new GeneralSettingsViewModel(),
                1 => new PreferencesSettingsViewModel(),
                2 => new PrivacySettingsViewModel(),
                3 => new AboutSettingsViewModel(),
                _ => new GeneralSettingsViewModel()
            };
        }

        private void NavigationView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
        {
            if (!ViewModel.Messenger.IsRegistered<NavigationMessage>(Navigation))
                ViewModel.Messenger.Register<NavigationMessage>(Navigation);

            var tag = Convert.ToInt32((e.SelectedItem as NavigationViewItem)?.Tag);
            var viewModel = GetViewModelForTag(tag);

            Navigation.Navigate(viewModel, new EntranceNavigationTransition());
        }

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            Hide();
            WeakReferenceMessenger.Default.Send(new DialogHiddenMessage());
        }
    }
}