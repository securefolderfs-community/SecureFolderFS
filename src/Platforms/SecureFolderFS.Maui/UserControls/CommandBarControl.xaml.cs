using System.Windows.Input;

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

        private void MainContent_Loaded(object? sender, EventArgs e)
        {
            UpdateToolbarOnTop(IsToolbarOnTop);
        }

        private void UpdateToolbarOnTop(bool value)
        {
            if (value)
            {
                TopBorder.Background = Colors.Transparent;
                Grid.SetRowSpan(TopBorder, 1);
                Grid.SetRowSpan(MainContent, 1);
                Grid.SetRow(MainContent, 1);
            }
            else
            {
                TopBorder.Background = Resources["BarGradient"] as Brush;
                Grid.SetRowSpan(TopBorder, 2);
                Grid.SetRow(MainContent, 0);
                Grid.SetRowSpan(MainContent, 2);
            }
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
                        await commandBar.TopBorder.TranslateTo(0, 0, 350U, Easing.CubicInOut);
                    }
                    else
                    {
                        await commandBar.TopBorder.TranslateTo(0, -150, 350U, Easing.CubicInOut);
                        commandBar.TopBorder.IsVisible = false;
                    }
                });

        public bool IsToolbarOnTop
        {
            get => (bool)GetValue(IsToolbarOnTopProperty);
            set => SetValue(IsToolbarOnTopProperty, value);
        }
        public static readonly BindableProperty IsToolbarOnTopProperty =
            BindableProperty.Create(nameof(IsToolbarOnTop), typeof(bool), typeof(CommandBarControl), propertyChanged:
                static (bindable, _, newValue) =>
                {
                    if (newValue is not bool bValue)
                        return;

                    if (bindable is not CommandBarControl commandBar)
                        return;

                    commandBar.UpdateToolbarOnTop(bValue);
                });
        
        public ICommand? PropertiesCommand
        {
            get => (ICommand?)GetValue(PropertiesCommandProperty);
            set => SetValue(PropertiesCommandProperty, value);
        }
        public static readonly BindableProperty PropertiesCommandProperty =
            BindableProperty.Create(nameof(PropertiesCommand), typeof(ICommand), typeof(CommandBarControl));
        
        public ICommand? ShareCommand
        {
            get => (ICommand?)GetValue(ShareCommandProperty);
            set => SetValue(ShareCommandProperty, value);
        }
        public static readonly BindableProperty ShareCommandProperty =
            BindableProperty.Create(nameof(ShareCommand), typeof(ICommand), typeof(CommandBarControl));

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

