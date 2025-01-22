using System.Windows.Input;
using AndroidX.Lifecycle;
using SecureFolderFS.Sdk.ViewModels.Views.Browser;

namespace SecureFolderFS.Maui.UserControls
{
    public partial class BrowserControl : ContentView
    {
        public BrowserControl()
        {
            InitializeComponent();
        }
        
        private void RefreshView_Refreshing(object? sender, EventArgs e)
        {
            if (sender is not RefreshView refreshView)
                return;

            RefreshCommand?.Execute(null);
            refreshView.IsRefreshing = false;
        }
        
        private void TapGestureRecognizer_Tapped(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is not View { BindingContext: BrowserItemViewModel itemViewModel })
                return;

            if (IsSelecting)
                itemViewModel.IsSelected = !itemViewModel.IsSelected;
            else
                itemViewModel.OpenCommand.Execute(null);
        }
        
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
    }
}

