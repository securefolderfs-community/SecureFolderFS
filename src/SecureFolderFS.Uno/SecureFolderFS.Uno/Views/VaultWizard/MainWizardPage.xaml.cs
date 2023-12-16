using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Views.VaultWizard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWizardPage : Page
    {
        public MainWizardPageViewModel ViewModel
        {
            get => (MainWizardPageViewModel)DataContext;
            set => DataContext = value;
        }

        public MainWizardPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is MainWizardPageViewModel viewModel)
                ViewModel = viewModel;

            await UpdateChoice(default);
            base.OnNavigatedTo(e);
        }

        private async void Segmented_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await UpdateChoice(default);
        }

        private async Task UpdateChoice(CancellationToken cancellationToken)
        {
            //if (ViewModel is null || SegmentedControl.SelectedItem is not SegmentedItem segmentedItem)
            //    return;

            //var creationType = (string)segmentedItem.Tag == "CREATE" ? NewVaultCreationType.CreateNew : NewVaultCreationType.AddExisting;

            //await ViewModel.UpdateSelectionAsync(creationType, cancellationToken);
        }
    }
}
