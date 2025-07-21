using System.Collections.ObjectModel;
using SecureFolderFS.Maui.AppModels;

namespace SecureFolderFS.Maui.Views
{
    public partial class ContentPageExtended : ContentPage
    {
        public ContentPageExtended()
        {
            ExToolbarItems = new ObservableCollection<ExMenuItemBase>();
            InitializeComponent();
        }

        public IList<ExMenuItemBase> ExToolbarItems
        {
            get => (IList<ExMenuItemBase>)GetValue(ExToolbarItemsProperty);
            set => SetValue(ExToolbarItemsProperty, value);
        }
        public static readonly BindableProperty ExToolbarItemsProperty =
            BindableProperty.Create(nameof(ExToolbarItems), typeof(IList<ExMenuItemBase>), typeof(ContentPageExtended), null);
    }
}

