using System.Windows.Input;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared;
using SecureFolderFS.UI;

namespace SecureFolderFS.Maui.UserControls.Browser
{
    public partial class BrowserControl : ContentView
    {
        public BrowserControl()
        {
            _deferredInitialization = new(Constants.Browser.THUMBNAIL_MAX_PARALLELISATION);
            _settingsService = DI.Service<ISettingsService>();
            InitializeComponent();
        }

        private void RefreshView_Refreshing(object? sender, EventArgs e)
        {
            if (sender is not RefreshView refreshView)
                return;

            RefreshCommand?.Execute(null);
            refreshView.IsRefreshing = false;
        }

        public object? EmptyView
        {
            get => (object?)GetValue(EmptyViewProperty);
            set => SetValue(EmptyViewProperty, value);
        }
        public static readonly BindableProperty EmptyViewProperty =
            BindableProperty.Create(nameof(EmptyView), typeof(object), typeof(BrowserControl), defaultValue: null);

        public bool IsSelecting
        {
            get => (bool)GetValue(IsSelectingProperty);
            set => SetValue(IsSelectingProperty, value);
        }
        public static readonly BindableProperty IsSelectingProperty =
            BindableProperty.Create(nameof(IsSelecting), typeof(bool), typeof(BrowserControl), defaultValue: false);

        public ICommand? RefreshCommand
        {
            get => (ICommand?)GetValue(RefreshCommandProperty);
            set => SetValue(RefreshCommandProperty, value);
        }
        public static readonly BindableProperty RefreshCommandProperty =
            BindableProperty.Create(nameof(RefreshCommand), typeof(ICommand), typeof(BrowserControl), defaultValue: null);

        public IList<BrowserItemViewModel>? ItemsSource
        {
            get => (IList<BrowserItemViewModel>?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IList<BrowserItemViewModel>), typeof(BrowserControl), defaultValue: null);

        public BrowserViewType ViewType
        {
            get => (BrowserViewType)GetValue(ViewTypeProperty);
            set => SetValue(ViewTypeProperty, value);
        }
        public static readonly BindableProperty ViewTypeProperty =
            BindableProperty.Create(nameof(ViewType), typeof(BrowserViewType), typeof(BrowserControl), defaultValue: BrowserViewType.ListView);
    }
}
