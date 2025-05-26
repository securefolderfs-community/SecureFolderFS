namespace SecureFolderFS.Maui.UserControls
{
    public partial class CommandBarControl : ContentView
    {
        public event EventHandler? Closed;

        public CommandBarControl()
        {
            InitializeComponent();
        }

        private void Close_Clicked(object? sender, EventArgs e)
        {
            Closed?.Invoke(this, e);
        }
        
        public bool IsImmersed
        {
            get => (bool)GetValue(IsImmersedProperty);
            set => SetValue(IsImmersedProperty, value);
        }
        public static readonly BindableProperty IsImmersedProperty =
            BindableProperty.Create(nameof(IsImmersed), typeof(bool), typeof(CommandBarControl), propertyChanged:
                static async (bindable, _, newValue) =>
                {
                    if (newValue is not bool bValue)
                        return;
                    
                    if (bindable is not CommandBarControl commandBar)
                        return;

                    if (bValue)
                    {
                        commandBar.TopBorder.IsVisible = true;
                        await commandBar.TopBorder.TranslateTo(0, 0, 300U, Easing.CubicOut);
                    }
                    else
                    {
                        await commandBar.TopBorder.TranslateTo(0, -100, 300U, Easing.CubicIn);
                        commandBar.TopBorder.IsVisible = false;
                    }
                });
        
        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(CommandBarControl));
        
        public object? InnerContent
        {
            get => (object?)GetValue(InnerContentProperty);
            set => SetValue(InnerContentProperty, value);
        }
        public static readonly BindableProperty InnerContentProperty =
            BindableProperty.Create(nameof(InnerContent), typeof(object), typeof(CommandBarControl));
    }
}

