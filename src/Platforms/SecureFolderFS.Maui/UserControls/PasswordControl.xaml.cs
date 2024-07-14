namespace SecureFolderFS.Maui.UserControls
{
    public partial class PasswordControl : ContentView
    {
        public PasswordControl()
        {
            BindingContext = Root;
            InitializeComponent();
        }

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(PasswordControl), defaultValue: null);

        public string? Placeholder
        {
            get => (string?)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }
        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(PasswordControl), defaultValue: null);

        public string? UnsecurePassword
        {
            get => (string?)GetValue(UnsecurePasswordProperty);
            set => SetValue(UnsecurePasswordProperty, value);
        }
        public static readonly BindableProperty UnsecurePasswordProperty =
            BindableProperty.Create(nameof(UnsecurePassword), typeof(string), typeof(PasswordControl), defaultValue: null);
    }
}
