using System.Windows.Input;

namespace SecureFolderFS.Maui.UserControls.Browser
{
    public partial class ItemProperty : ContentView
    {
        public ItemProperty()
        {
            InitializeComponent();
        }
        
        public bool IsLastItem
        {
            get => (bool)GetValue(IsLastItemProperty);
            set => SetValue(IsLastItemProperty, value);
        }
        public static readonly BindableProperty IsLastItemProperty =
            BindableProperty.Create(nameof(IsLastItem), typeof(bool), typeof(ItemProperty), false);

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(ItemProperty));

        public string? Subtitle
        {
            get => (string?)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }
        public static readonly BindableProperty SubtitleProperty =
            BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(ItemProperty));

        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ItemProperty));

        public object? CommandParameter
        {
            get => (object?)GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }
        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ItemProperty));
    }
}

