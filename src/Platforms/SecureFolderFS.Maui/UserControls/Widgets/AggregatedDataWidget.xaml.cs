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
            BindableProperty.Create(nameof(TotalRead), typeof(string), typeof(AggregatedDataWidget));

        public string? TotalWrite
        {
            get => (string?)GetValue(TotalWriteProperty);
            set => SetValue(TotalWriteProperty, value);
        }
        public static readonly BindableProperty TotalWriteProperty =
            BindableProperty.Create(nameof(TotalWrite), typeof(string), typeof(AggregatedDataWidget));

        public bool IsReading
        {
            get => (bool)GetValue(IsReadingProperty);
            set => SetValue(IsReadingProperty, value);
        }
        public static readonly BindableProperty IsReadingProperty =
            BindableProperty.Create(nameof(IsReading), typeof(bool), typeof(AggregatedDataWidget), false,
                propertyChanged: static (bindable, _, newValue) =>
                {
                    if (bindable is not AggregatedDataWidget widget || newValue is not bool bValue)
                        return;

                    widget.ReadEllipsis.FadeToAsync(bValue ? 1d : 0d);
                });

        public bool IsWriting
        {
            get => (bool)GetValue(IsWritingProperty);
            set => SetValue(IsWritingProperty, value);
        }
        public static readonly BindableProperty IsWritingProperty =
            BindableProperty.Create(nameof(IsWriting), typeof(bool), typeof(AggregatedDataWidget), false,
                propertyChanged: static (bindable, _, newValue) =>
                {
                    if (bindable is not AggregatedDataWidget widget || newValue is not bool bValue)
                        return;

                    widget.WriteEllipsis.FadeToAsync(bValue ? 1d : 0d);
                });
    }
}

