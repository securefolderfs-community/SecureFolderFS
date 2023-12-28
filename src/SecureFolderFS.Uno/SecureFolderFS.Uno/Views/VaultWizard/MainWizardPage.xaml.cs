using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Views.VaultWizard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [INotifyPropertyChanged]
    public sealed partial class MainWizardPage : Page
    {
        private Button? _lastClickedButton;

        public MainWizardPageViewModel ViewModel
        {
            get => (MainWizardPageViewModel)DataContext;
            set { DataContext = value; OnPropertyChanged(); }
        }

        public MainWizardPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is MainWizardPageViewModel viewModel)
                ViewModel = viewModel;

            await ViewModel.UpdateSelectionAsync(NewVaultCreationType.CreateNew, default);
            base.OnNavigatedTo(e);
        }

        private async void SegmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: string tag } button)
                return;

            _lastClickedButton ??= CreateNewButton;
            _lastClickedButton.Style = (Style?)App.Instance?.Resources["DefaultButtonStyle"];

            var creationType = tag == "CREATE" ? NewVaultCreationType.CreateNew : NewVaultCreationType.AddExisting;
            await ViewModel.UpdateSelectionAsync(creationType, default);

            button.Style = (Style?)App.Instance?.Resources["AccentButtonStyle"];
            _lastClickedButton = button;
        }
    }
}
