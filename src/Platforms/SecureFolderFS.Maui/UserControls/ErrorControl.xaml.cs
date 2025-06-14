namespace SecureFolderFS.Maui.UserControls
{
    public partial class ErrorControl : ContentView
    {
        public ErrorControl()
        {
            InitializeComponent();
        }
        
        public string? ExceptionMessage
        {
            get => (string?)GetValue(ExceptionMessageProperty);
            set => SetValue(ExceptionMessageProperty, value);
        }
        public static readonly BindableProperty ExceptionMessageProperty =
            BindableProperty.Create(nameof(ExceptionMessage), typeof(string), typeof(ErrorControl));
    }
}

