using SecureFolderFS.Sdk.ViewModels.Views.Browser;

namespace SecureFolderFS.Maui.UserControls
{
    public partial class BrowserControl : ContentView
    {
        public BrowserControl()
        {
            InitializeComponent();
        }
        
        public IList<BrowserItemViewModel>? ItemsSource
        {
            get => (IList<BrowserItemViewModel>?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IList<BrowserItemViewModel>), typeof(BrowserControl), defaultValue: null);
    }
}

