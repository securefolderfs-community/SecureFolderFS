using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Dialogs
{
    public sealed partial class SettingsDialog : ContentDialog, IDialog<SettingsDialogViewModel>
    {
        /// <inheritdoc/>
        public SettingsDialogViewModel ViewModel
        {
            get => (SettingsDialogViewModel)DataContext;
            set => DataContext = value;
        }

        public SettingsDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<DialogResult> ShowAsync() => (DialogResult)await base.ShowAsync();

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

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (!ViewModel.Messenger.IsRegistered<NavigationMessage>(Navigation))
                ViewModel.Messenger.Register<NavigationMessage>(Navigation);

            var tag = Convert.ToInt32((args.SelectedItem as NavigationViewItem)?.Tag);
            var viewModel = GetViewModelForTag(tag);

            Navigation.Navigate(viewModel, new EntranceNavigationTransitionInfo());
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
