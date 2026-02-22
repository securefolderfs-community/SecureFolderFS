namespace SecureFolderFS.Maui.UserControls
{
    public partial class IntroductionSlide : ContentView
    {
        public IntroductionSlide()
        {
            InitializeComponent();
        }
        
        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(IntroductionSlide));
        
        public string? Subtitle
        {
            get => (string?)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }
        public static readonly BindableProperty SubtitleProperty =
            BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(IntroductionSlide));
        
        public object? Slot
        {
            get => (object?)GetValue(SlotProperty);
            set => SetValue(SlotProperty, value);
        }
        public static readonly BindableProperty SlotProperty =
            BindableProperty.Create(nameof(Slot), typeof(object), typeof(IntroductionSlide));
        
        public object? SlotBelow
        {
            get => (object?)GetValue(SlotBelowProperty);
            set => SetValue(SlotBelowProperty, value);
        }
        public static readonly BindableProperty SlotBelowProperty =
            BindableProperty.Create(nameof(SlotBelow), typeof(object), typeof(IntroductionSlide));
    }
}
