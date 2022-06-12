using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Pages.SettingsDialog;
using SecureFolderFS.WinUI.Views.Settings;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Dialogs
{
    public sealed partial class SettingsDialog : ContentDialog, IDialog<SettingsDialogViewModel>, IRecipient<SettingsNavigationRequestedMessage>
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
        
        public void Receive(SettingsNavigationRequestedMessage message)
        {
            Navigate(message.Value);
        }

        private void Navigate(BaseSettingsDialogPageViewModel viewModel)
        {
            switch (viewModel)
            {
                case GeneralSettingsPageViewModel:
                    ContentFrame.Navigate(typeof(GeneralSettingsPage), viewModel, new EntranceNavigationTransitionInfo());
                    break;

                case PreferencesSettingsPageViewModel:
                    ContentFrame.Navigate(typeof(PreferencesSettingsPage), viewModel, new EntranceNavigationTransitionInfo());
                    break;

                case SecuritySettingsPageViewModel:
                    ContentFrame.Navigate(typeof(SecuritySettingsPage), viewModel, new EntranceNavigationTransitionInfo());
                    break;

                case AboutSettingsPageViewModel:
                    ContentFrame.Navigate(typeof(AboutSettingsPage), viewModel, new EntranceNavigationTransitionInfo());
                    break;
            }
        }

        private void SettingsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Messenger.Register<SettingsNavigationRequestedMessage>(this);
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var tag = Convert.ToInt32((args.SelectedItem as NavigationViewItem)?.Tag);

            switch (tag)
            {
                default:
                case 0:
                    Navigate(new GeneralSettingsPageViewModel());
                    break;

                case 1:
                    Navigate(new PreferencesSettingsPageViewModel());
                    break;

                case 2:
                    Navigate(new SecuritySettingsPageViewModel());
                    break;

                case 3:
                    Navigate(new AboutSettingsPageViewModel());
                    break;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
