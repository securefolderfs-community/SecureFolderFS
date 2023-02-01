using System;
using System.ComponentModel;
using System.Threading.Tasks;
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
                0 => new GeneralSettingsPageViewModel(),
                1 => new PreferencesSettingsPageViewModel(),
                2 => new PrivacySettingsPageViewModel(),
                3 => new AboutSettingsPageViewModel(),
                _ => new GeneralSettingsPageViewModel()
            };
        }

        private void NavigationView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
        {
            if (!ViewModel.Messenger.IsRegistered<NavigationRequestedMessage>(Navigation))
                ViewModel.Messenger.Register<NavigationRequestedMessage>(Navigation);

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