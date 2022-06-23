using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Models.Transitions;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Pages.SettingsDialog;
using SecureFolderFS.WinUI.Views.Settings;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Dialogs
{
    public sealed partial class SettingsDialog : ContentDialog, IDialog<SettingsDialogViewModel>
    {
        private bool _navigationInitialized;

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

        private BaseSettingsDialogPageViewModel GetViewModelForTag(int tag)
        {
            return tag switch
            {
                0 => new GeneralSettingsPageViewModel(),
                1 => new PreferencesSettingsPageViewModel(),
                2 => new SecuritySettingsPageViewModel(),
                3 => new AboutSettingsPageViewModel(),
                _ => new GeneralSettingsPageViewModel()
            };
        }

        private void EnsureNavigationInitialized()
        {
            if (_navigationInitialized)
                return;

            _navigationInitialized = true;

            ViewModel.Messenger.Register(Navigation);
            Navigation.ViewModelAssociation = new()
            {
                { typeof(GeneralSettingsPageViewModel), typeof(GeneralSettingsPage) },
                { typeof(PreferencesSettingsPageViewModel), typeof(PreferencesSettingsPage) },
                { typeof(SecuritySettingsPageViewModel), typeof(SecuritySettingsPage) },
                { typeof(AboutSettingsPageViewModel), typeof(AboutSettingsPage) }
            };
        }

        private void Navigation_Loaded(object sender, RoutedEventArgs e)
        {
            EnsureNavigationInitialized();
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            EnsureNavigationInitialized();

            var tag = Convert.ToInt32((args.SelectedItem as NavigationViewItem)?.Tag);
            var viewModel = GetViewModelForTag(tag);
            ViewModel.Messenger.Send(new NavigationRequestedMessage(viewModel, new EntranceTransitionModel())); // TODO: Just for testing.
            //Navigation.Navigate(viewModel, new EntranceTransitionModel()); // EntranceNavigationTransitionInfo
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
