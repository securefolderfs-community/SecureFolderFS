using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Views.Settings;
using SecureFolderFS.Shared.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Views.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [INotifyPropertyChanged]
    public sealed partial class AccountsSettingsPage : Page
    {
        public AccountsSettingsViewModel? ViewModel
        {
            get => DataContext.TryCast<AccountsSettingsViewModel>();
            set { DataContext = value; OnPropertyChanged(); }
        }
        
        public AccountsSettingsPage()
        {
            InitializeComponent();
        }
        
        /// <inheritdoc/>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is AccountsSettingsViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);

            // Refresh the account list every time the page is shown so it stays up to date.
            if (ViewModel is not null)
                await ViewModel.InitAsync();
        }
    }
}
