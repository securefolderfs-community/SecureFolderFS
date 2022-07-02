using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Pages.Settings;

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
                0 => new GeneralSettingsPageViewModel(),
                1 => new PreferencesSettingsPageViewModel(),
                2 => new PrivacySettingsPageViewModel(),
                3 => new AboutSettingsPageViewModel(),
                _ => new GeneralSettingsPageViewModel()
            };
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (!ViewModel.Messenger.IsRegistered<NavigationRequestedMessage>(Navigation))
                ViewModel.Messenger.Register<NavigationRequestedMessage>(Navigation);

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
