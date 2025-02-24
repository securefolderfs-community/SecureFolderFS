namespace SecureFolderFS.Maui.UserControls.Widgets
{
    public partial class AggregatedDataWidget : ContentView
    {
        public AggregatedDataWidget()
        {
            InitializeComponent();
        }

        public string? TotalRead
        {
            get => (string?)GetValue(TotalReadProperty);
            set => SetValue(TotalReadProperty, value);
        }
        public static readonly BindableProperty TotalReadProperty =
            BindableProperty.Create(nameof(TotalRead), typeof(string), typeof(AggregatedDataWidget), null);

        public string? TotalWrite
        {
            get => (string?)GetValue(TotalWriteProperty);
            set => SetValue(TotalWriteProperty, value);
        }
        public static readonly BindableProperty TotalWriteProperty =
            BindableProperty.Create(nameof(TotalWrite), typeof(string), typeof(AggregatedDataWidget), null);
    }
}

