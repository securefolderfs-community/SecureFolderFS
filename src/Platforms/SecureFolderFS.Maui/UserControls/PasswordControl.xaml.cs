using SecureFolderFS.Sdk.Extensions;

namespace SecureFolderFS.Maui.UserControls
{
    public partial class PasswordControl : ContentView
    {
        public PasswordControl()
        {
            InitializeComponent();
            RootGrid.BindingContext = this;
        }

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(PasswordControl), defaultValue: "EnterPassword".ToLocalized());

        public string? Placeholder
        {
            get => (string?)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }
        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(PasswordControl), defaultValue: "Password".ToLocalized());

        public string? UnsecurePassword
        {
            get => (string?)GetValue(UnsecurePasswordProperty);
            set => SetValue(UnsecurePasswordProperty, value);
        }
        public static readonly BindableProperty UnsecurePasswordProperty =
            BindableProperty.Create(nameof(UnsecurePassword), typeof(string), typeof(PasswordControl), defaultValue: null, defaultBindingMode: BindingMode.TwoWay);
    }
}
