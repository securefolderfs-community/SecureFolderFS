using SecureFolderFS.Sdk.ViewModels.Controls;

namespace SecureFolderFS.Maui.UserControls
{
    public partial class BreadcrumbBar : ContentView
    {
        public BreadcrumbBar()
        {
            InitializeComponent();
        }
        
        public IList<BreadcrumbItemViewModel>? ItemsSource
        {
            get => (IList<BreadcrumbItemViewModel>?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IList<BreadcrumbItemViewModel>), typeof(BreadcrumbBar), defaultValue: null);
    }
}
