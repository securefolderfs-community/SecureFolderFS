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
            ViewModel = query.ToViewModel<VaultOverviewViewModel>()!;
            OnPropertyChanged(nameof(ViewModel));
        }

        public VaultOverviewViewModel ViewModel
        {
            get => (VaultOverviewViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(VaultOverviewViewModel), typeof(OverviewPage), null);
    }
}
