using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Maui.ServiceImplementation;
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

        /// <inheritdoc/>
        protected override void OnAppearing()
        {
            if (ViewModel?.VaultNavigation is MauiNavigationService navigationService)
                navigationService.SetCurrentViewInternal(ViewModel);
            
            base.OnAppearing();
        }

        public VaultDashboardViewModel? ViewModel
        {
            get => (VaultDashboardViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(VaultDashboardViewModel), typeof(OverviewPage));

        public VaultOverviewViewModel? OverviewViewModel
        {
            get => (VaultOverviewViewModel?)GetValue(OverviewViewModelProperty);
            set => SetValue(OverviewViewModelProperty, value);
        }
        public static readonly BindableProperty OverviewViewModelProperty =
            BindableProperty.Create(nameof(OverviewViewModel), typeof(VaultOverviewViewModel), typeof(OverviewPage));

        public VaultPropertiesViewModel? PropertiesViewModel
        {
            get => (VaultPropertiesViewModel?)GetValue(PropertiesViewModelProperty);
            set => SetValue(PropertiesViewModelProperty, value);
        }
        public static readonly BindableProperty PropertiesViewModelProperty =
            BindableProperty.Create(nameof(PropertiesViewModel), typeof(VaultPropertiesViewModel), typeof(OverviewPage));
    }
}
