namespace SecureFolderFS.Maui.UserControls
{
    public partial class FeatureTile : ContentView
    {
        public FeatureTile()
        {
            InitializeComponent();
        }

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(FeatureTile));

        public string? Description
        {
            get => (string?)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }
        public static readonly BindableProperty DescriptionProperty =
            BindableProperty.Create(nameof(Description), typeof(string), typeof(FeatureTile));

        public View? IconContent
        {
            get => (View?)GetValue(IconContentProperty);
            set => SetValue(IconContentProperty, value);
        }
        public static readonly BindableProperty IconContentProperty =
            BindableProperty.Create(nameof(IconContent), typeof(View), typeof(FeatureTile));
    }
}

