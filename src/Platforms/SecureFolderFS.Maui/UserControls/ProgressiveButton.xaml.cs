using MauiIcons.Material;

namespace SecureFolderFS.Maui.UserControls
{
    public partial class ProgressiveButton : ContentView
    {
        public ProgressiveButton()
        {
            InitializeComponent();
        }
        
        private void LayoutProgressStrip()
        {
            var adjustedProgress = Math.Max(Math.Min(Progress, 100d), 0d);
            var maxWidth = ProgressContainer.Width;
            var width = (adjustedProgress / 100) * maxWidth;

            ProgressStrip.WidthRequest = width;
        }
        
        private void ProgressContainer_SizeChanged(object? sender, EventArgs e)
        {
            LayoutProgressStrip();
        }
        
        public Enum? DefaultIcon
        {
            get => (Enum?)GetValue(DefaultIconProperty);
            set => SetValue(DefaultIconProperty, value);
        }
        public static readonly BindableProperty DefaultIconProperty =
            BindableProperty.Create(nameof(DefaultIcon), typeof(Enum), typeof(ProgressiveButton), MaterialIcons.Search);
        
        public bool IsProgressing
        {
            get => (bool)GetValue(IsProgressingProperty);
            set => SetValue(IsProgressingProperty, value);
        }
        public static readonly BindableProperty IsProgressingProperty =
            BindableProperty.Create(nameof(IsProgressing), typeof(bool), typeof(ProgressiveButton), false);
        
        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }
        public static readonly BindableProperty ProgressProperty =
            BindableProperty.Create(nameof(Progress), typeof(double), typeof(ProgressiveButton), 0d, propertyChanged:
                static (bindable, value, newValue) =>
                {
                    if (bindable is not ProgressiveButton progressiveButton)
                        return;
                    
                    progressiveButton.LayoutProgressStrip();
                });
    }
}

