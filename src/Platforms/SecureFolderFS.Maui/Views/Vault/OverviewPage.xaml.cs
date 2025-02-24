using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Maui.Views.Vault
{
    public partial class OverviewPage : ContentPage, IQueryAttributable
    {
        public OverviewPage()
        {
            BindingContext = this;
            InitializeComponent();
        }

        /// <inheritdoc/>
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            ViewModel = query.ToViewModel<VaultDashboardViewModel>();
            if (ViewModel is not null)
            {
                OverviewViewModel = ViewModel.DashboardNavigation.Views.FirstOrDefaultType<IViewDesignation, VaultOverviewViewModel>();
                PropertiesViewModel = ViewModel.DashboardNavigation.Views.FirstOrDefaultType<IViewDesignation, VaultPropertiesViewModel>();
            }
            
            OnPropertyChanged(nameof(ViewModel));
            OnPropertyChanged(nameof(OverviewViewModel));
            OnPropertyChanged(nameof(PropertiesViewModel));
        }

        public VaultDashboardViewModel? ViewModel
        {
            get => (VaultDashboardViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(VaultDashboardViewModel), typeof(OverviewPage), null);

        public VaultOverviewViewModel? OverviewViewModel
        {
            get => (VaultOverviewViewModel?)GetValue(OverviewViewModelProperty);
            set => SetValue(OverviewViewModelProperty, value);
        }
        public static readonly BindableProperty OverviewViewModelProperty =
            BindableProperty.Create(nameof(OverviewViewModel), typeof(VaultOverviewViewModel), typeof(OverviewPage), null);

        public VaultPropertiesViewModel? PropertiesViewModel
        {
            get => (VaultPropertiesViewModel?)GetValue(PropertiesViewModelProperty);
            set => SetValue(PropertiesViewModelProperty, value);
        }
        public static readonly BindableProperty PropertiesViewModelProperty =
            BindableProperty.Create(nameof(PropertiesViewModel), typeof(VaultPropertiesViewModel), typeof(OverviewPage), null);
    }
}
