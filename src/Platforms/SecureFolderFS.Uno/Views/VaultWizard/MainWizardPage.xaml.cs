using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard2;
using SecureFolderFS.Shared.Extensions;

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
        private readonly LocationWizardViewModel _createNewViewModel = new(NewVaultCreationType.CreateNew);
        private readonly LocationWizardViewModel _addExistingViewModel = new(NewVaultCreationType.AddExisting);
        private Button? _lastClickedButton;

        [ObservableProperty] private LocationWizardViewModel? _CurrentViewModel;

        public MainWizardViewModel? ViewModel
        {
            get => DataContext.TryCast<MainWizardViewModel>();
            set { DataContext = value; OnPropertyChanged(); }
        }

        public MainWizardPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is MainWizardViewModel viewModel)
            {
                ViewModel = viewModel;
                ViewModel.CreationType = NewVaultCreationType.CreateNew; // Default value for the view
                CurrentViewModel = _createNewViewModel;
                await CurrentViewModel.UpdateStatusAsync();
            }

            base.OnNavigatedTo(e);
        }

        private async void SegmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel is null || sender is not Button { Tag: string tag } button)
                return;

            // Apply styles
            _lastClickedButton ??= CreateNewButton;
            _lastClickedButton.Style = (Style?)App.Instance?.Resources["DefaultButtonStyle"];

            // Change type
            ViewModel.CreationType = tag == "CREATE" ? NewVaultCreationType.CreateNew : NewVaultCreationType.AddExisting;
            CurrentViewModel = ViewModel.CreationType == NewVaultCreationType.CreateNew ? _createNewViewModel : _addExistingViewModel;
            await CurrentViewModel.UpdateStatusAsync();

            // Apply styles
            button.Style = (Style?)App.Instance?.Resources["AccentButtonStyle"];
            _lastClickedButton = button;
        }
    }
}
