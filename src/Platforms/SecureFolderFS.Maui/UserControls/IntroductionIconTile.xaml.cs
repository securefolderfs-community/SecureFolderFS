namespace SecureFolderFS.Maui.UserControls
{
    public partial class IntroductionIconTile : ContentView
    {
        public IntroductionIconTile()
        {
            InitializeComponent();
        }

        public ImageSource? Source
        {
            get => (ImageSource?)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }
        public static readonly BindableProperty SourceProperty =
            BindableProperty.Create(nameof(Source), typeof(ImageSource), typeof(IntroductionIconTile));
    }
}

