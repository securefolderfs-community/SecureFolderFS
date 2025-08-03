using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Shared;
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
        private PickerSourceWizardViewModel? _createNewViewModel;
        private PickerSourceWizardViewModel? _addExistingViewModel;
        private Button? _lastClickedButton;

        [ObservableProperty] private PickerSourceWizardViewModel? _CurrentViewModel;

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
                var fileExplorerService = DI.Service<IFileExplorerService>();

                ViewModel = viewModel;
                ViewModel.Mode = NewVaultMode.CreateNew; // Default value for the view
                _createNewViewModel = new(fileExplorerService, NewVaultMode.CreateNew, viewModel.VaultCollectionModel);
                _addExistingViewModel = new(fileExplorerService, NewVaultMode.AddExisting, viewModel.VaultCollectionModel);
                CurrentViewModel = _createNewViewModel;
                ViewModel.CanContinue = await CurrentViewModel.UpdateStatusAsync();
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (CurrentViewModel is not null)
                CurrentViewModel.PropertyChanged -= CurrentViewModel_PropertyChanged;

            base.OnNavigatingFrom(e);
        }

        partial void OnCurrentViewModelChanged(PickerSourceWizardViewModel? oldValue, PickerSourceWizardViewModel? newValue)
        {
            if (oldValue is not null)
                oldValue.PropertyChanged -= CurrentViewModel_PropertyChanged;

            if (newValue is not null)
                newValue.PropertyChanged += CurrentViewModel_PropertyChanged;
        }

        private void CurrentViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (ViewModel is not null && CurrentViewModel is not null && e.PropertyName == nameof(IOverlayControls.CanContinue))
                ViewModel.CanContinue = CurrentViewModel.CanContinue;
        }

        private async void SegmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel is null || sender is not Button { Tag: string tag } button)
                return;

            // Apply styles
            _lastClickedButton ??= CreateNewButton;
            _lastClickedButton.Style = (Style?)App.Instance?.Resources["DefaultButtonStyle"];

            // Change type
            ViewModel.Mode = tag == "CREATE" ? NewVaultMode.CreateNew : NewVaultMode.AddExisting;
            CurrentViewModel = ViewModel.Mode == NewVaultMode.CreateNew ? _createNewViewModel : _addExistingViewModel;
            if (CurrentViewModel is not null)
                ViewModel.CanContinue = await CurrentViewModel.UpdateStatusAsync();

            // Apply styles
            button.Style = (Style?)App.Instance?.Resources["AccentButtonStyle"];
            _lastClickedButton = button;
        }
    }
}
