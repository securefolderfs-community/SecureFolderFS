using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;

namespace SecureFolderFS.Maui.Views.Vault
{
    public partial class OverviewPage : ContentPage, IQueryAttributable
    {
        public OverviewPage()
        {
            BindingContext = this;
            _ = new MauiIcons.Core.MauiIcon(); // Workaround for XFC0000

            InitializeComponent();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            ViewModel = query.ToViewModel<VaultOverviewPageViewModel>()!;
            OnPropertyChanged(nameof(ViewModel));
        }

        public VaultOverviewPageViewModel ViewModel
        {
            get => (VaultOverviewPageViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(VaultOverviewPageViewModel), typeof(OverviewPage), null);
    }
}
