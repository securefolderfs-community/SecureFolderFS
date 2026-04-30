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

        public string? Subtitle
        {
            get => (string?)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }
        public static readonly BindableProperty SubtitleProperty =
            BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(OptionsContainer));

        public string? Description
        {
            get => (string?)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }
        public static readonly BindableProperty DescriptionProperty =
            BindableProperty.Create(nameof(Description), typeof(string), typeof(OptionsContainer));

        public object? InnerContent
        {
            get => (object?)GetValue(InnerContentProperty);
            set => SetValue(InnerContentProperty, value);
        }
        public static readonly BindableProperty InnerContentProperty =
            BindableProperty.Create(nameof(InnerContent), typeof(object), typeof(OptionsContainer));

        public bool ProvideBackplate
        {
            get => (bool)GetValue(ProvideBackplateProperty);
            set => SetValue(ProvideBackplateProperty, value);
        }
        public static readonly BindableProperty ProvideBackplateProperty =
            BindableProperty.Create(nameof(ProvideBackplate), typeof(bool), typeof(OptionsContainer), true);
    }
}

