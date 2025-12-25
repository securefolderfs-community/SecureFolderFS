namespace SecureFolderFS.Maui.UserControls.Options
{
    public partial class OptionsContainer : ContentView
    {
        public OptionsContainer()
        {
            InitializeComponent();
        }

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(OptionsContainer));

        public object? InnerContent
        {
            get => (object?)GetValue(InnerContentProperty);
            set => SetValue(InnerContentProperty, value);
        }
        public static readonly BindableProperty InnerContentProperty =
            BindableProperty.Create(nameof(InnerContent), typeof(object), typeof(OptionsContainer));
        
        public bool IsTransparent
        {
            get => (bool)GetValue(IsTransparentProperty);
            set => SetValue(IsTransparentProperty, value);
        }
        public static readonly BindableProperty IsTransparentProperty =
            BindableProperty.Create(nameof(IsTransparent), typeof(bool), typeof(OptionsContainer), false,
                propertyChanged: (bindable, _, newValue) =>
                {
                    if (bindable is not OptionsContainer container)
                        return;

                    container.ContainerBorder.Background = (bool)newValue
                        ? Colors.Transparent
                        : App.Current.Resources["ThemeElevatedFillPrimaryColorBrush"] as Brush;
                });
    }
}

