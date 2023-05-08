using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views.VaultWizard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EncryptionWizardPage : Page
    {
        public EncryptionWizardViewModel ViewModel
        {
            get => (EncryptionWizardViewModel)DataContext;
            set => DataContext = value;
        }

        public EncryptionWizardPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            InitializeComponent();
            if (e.Parameter is EncryptionWizardViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
                comboBox.SelectedIndex = 0;
        }
    }
}
