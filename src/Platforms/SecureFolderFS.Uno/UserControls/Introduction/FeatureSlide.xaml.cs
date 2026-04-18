using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.Introduction
{
    public sealed partial class FeatureSlide : UserControl
    {
        public FeatureSlide()
        {
            InitializeComponent();
        }

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(FeatureSlide), new PropertyMetadata(null));

        public string? Description
        {
            get => (string?)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(FeatureSlide), new PropertyMetadata(null));

        public FrameworkElement? LeftSlot
        {
            get => (FrameworkElement?)GetValue(LeftSlotProperty);
            set => SetValue(LeftSlotProperty, value);
        }
        public static readonly DependencyProperty LeftSlotProperty =
            DependencyProperty.Register(nameof(LeftSlot), typeof(FrameworkElement), typeof(FeatureSlide), new PropertyMetadata(null));

        public FrameworkElement? RightSlot
        {
            get => (FrameworkElement?)GetValue(RightSlotProperty);
            set => SetValue(RightSlotProperty, value);
        }
        public static readonly DependencyProperty RightSlotProperty =
            DependencyProperty.Register(nameof(RightSlot), typeof(FrameworkElement), typeof(FeatureSlide), new PropertyMetadata(null));
    }
}
