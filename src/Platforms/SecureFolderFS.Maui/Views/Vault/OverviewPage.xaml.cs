using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;

namespace SecureFolderFS.Maui.Views.Vault
{
    public partial class OverviewPage : ContentPage, IQueryAttributable
    {
        public OverviewPage()
        {
            BindingContext = this;
            InitializeComponent();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            ViewModel = query.ToViewModel<VaultOverviewViewModel>();
            if (ViewModel is not null)
            {
                PropertiesViewModel = new(ViewModel.UnlockedVaultViewModel);
                _ = PropertiesViewModel.InitAsync();
            }
            
            OnPropertyChanged(nameof(ViewModel));
            OnPropertyChanged(nameof(PropertiesViewModel));
        }

        public VaultOverviewViewModel? ViewModel
        {
            get => (VaultOverviewViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(VaultOverviewViewModel), typeof(OverviewPage), null);
        
        public VaultPropertiesViewModel? PropertiesViewModel
        {
            get => (VaultPropertiesViewModel?)GetValue(PropertiesViewModelProperty);
            set => SetValue(PropertiesViewModelProperty, value);
        }
        public static readonly BindableProperty PropertiesViewModelProperty =
            BindableProperty.Create(nameof(PropertiesViewModel), typeof(VaultPropertiesViewModel), typeof(OverviewPage), null);
    }
}
