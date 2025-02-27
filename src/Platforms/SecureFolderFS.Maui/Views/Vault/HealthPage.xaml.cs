using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;

namespace SecureFolderFS.Maui.Views.Vault
{
    public partial class HealthPage : ContentPage, IQueryAttributable
    {
        public HealthPage()
        {
            BindingContext = this;
            InitializeComponent();
        }

        /// <inheritdoc/>
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            ViewModel = query.ToViewModel<VaultHealthReportViewModel>();
            OnPropertyChanged(nameof(ViewModel));
        }

        public VaultHealthReportViewModel? ViewModel
        {
            get => (VaultHealthReportViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(VaultHealthReportViewModel), typeof(HealthPage), null);
    }
}
