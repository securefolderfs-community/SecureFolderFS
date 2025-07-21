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

        private void ProgressiveButton_Clicked(object? sender, EventArgs e)
        {
            if (ViewModel is null)
                return;

            if (ViewModel.HealthViewModel.IsProgressing)
                ViewModel.HealthViewModel.CancelScanningCommand.Execute(null);
            else
                ViewModel.HealthViewModel.StartScanningCommand.Execute(null);
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
