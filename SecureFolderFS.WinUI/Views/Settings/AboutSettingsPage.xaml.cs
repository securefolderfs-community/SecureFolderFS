using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Pages.SettingsDialog;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AboutSettingsPage : Page
    {
        public AboutSettingsPageViewModel ViewModel
        {
            get => (AboutSettingsPageViewModel)DataContext;
            set => DataContext = value;
        }

        public AboutSettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is AboutSettingsPageViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            base.OnNavigatedTo(e);
        }

        private void VersionButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CopyVersionCommand?.Execute(null);
            VersionCopiedTeachingTip.IsOpen = true;
        }
    }
}
