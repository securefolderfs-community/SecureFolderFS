using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;

namespace SecureFolderFS.Maui.Views.Vault
{
    public partial class LoginPage : ContentPage, IQueryAttributable
    {
        public LoginPage()
        {
            BindingContext = this;
            _ = new MauiIcons.Core.MauiIcon(); // Workaround for XFC0000

            InitializeComponent();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            ViewModel = query.ToViewModel<VaultLoginViewModel>()!;
            OnPropertyChanged(nameof(ViewModel));
        }

        public VaultLoginViewModel ViewModel
        {
            get => (VaultLoginViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(VaultLoginViewModel), typeof(LoginPage), null);
    }
}
