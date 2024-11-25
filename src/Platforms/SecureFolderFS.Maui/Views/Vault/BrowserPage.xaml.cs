using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Browser;

namespace SecureFolderFS.Maui.Views.Vault
{
    public partial class BrowserPage : ContentPage, IQueryAttributable
    {
        public BrowserPage()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            ViewModel = query.ToViewModel<BrowserViewModel>();
            OnPropertyChanged(nameof(ViewModel));
        }
        
        public BrowserViewModel? ViewModel
        {
            get => (BrowserViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(BrowserViewModel), typeof(BrowserPage), null);
    }
}

