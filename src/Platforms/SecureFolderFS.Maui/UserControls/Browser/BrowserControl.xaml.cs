using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared;

namespace SecureFolderFS.Maui.UserControls.Browser
{
    public partial class BrowserControl : ContentView
    {
        public BrowserControl()
        {
            _thumbnailSemaphore = new SemaphoreSlim(4, 4);
            _thumbnailCts = new CancellationTokenSource();
            _settingsService = DI.Service<ISettingsService>();
            InitializeComponent();
        }

        private async void RefreshView_Refreshing(object? sender, EventArgs e)
        {
            if (sender is not RefreshView refreshView)
                return;

            try
            {
                // Keep the spinner visible until the refresh actually completes
                if (RefreshCommand is IAsyncRelayCommand asyncRefreshCommand)
                    await asyncRefreshCommand.ExecuteAsync(null);
                else
                    RefreshCommand?.Execute(null);
            }
            finally
            {
                refreshView.IsRefreshing = false;
            }
        }

        public BrowserViewModel? ViewModel
        {
            get => (BrowserViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(BrowserViewModel), typeof(BrowserControl), defaultValue: null);

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }
        public static readonly BindableProperty IsReadOnlyProperty =
            BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(BrowserControl), defaultValue: false);

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
            BindableProperty.Create(nameof(IsSelecting), typeof(bool), typeof(BrowserControl), defaultValue: false,
                propertyChanged: static (bindable, _, _) =>
                {
                    if (bindable is BrowserControl control)
                        control.UpdateAllItemContainerPanGestures();
                });

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
            BindableProperty.Create(nameof(ItemsSource), typeof(IList<BrowserItemViewModel>), typeof(BrowserControl), defaultValue: null,
                propertyChanged: static (bindable, oldValue, newValue) =>
                {
                    if (bindable is not BrowserControl control || ReferenceEquals(oldValue, newValue))
                        return;

                    // Cancel any in-flight thumbnail work from the previous folder
                    control._thumbnailCts?.Cancel();
                    control._thumbnailCts?.Dispose();
                    control._thumbnailCts = new CancellationTokenSource();
                });

        public BrowserViewType ViewType
        {
            get => (BrowserViewType)GetValue(ViewTypeProperty);
            set => SetValue(ViewTypeProperty, value);
        }
        public static readonly BindableProperty ViewTypeProperty =
            BindableProperty.Create(nameof(ViewType), typeof(BrowserViewType), typeof(BrowserControl), defaultValue: BrowserViewType.ListView);
    }
}
