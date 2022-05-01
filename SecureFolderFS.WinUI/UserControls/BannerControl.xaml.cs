using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls
{
    public sealed partial class BannerControl : UserControl
    {
        public BannerControl()
        {
            this.InitializeComponent();
        }

        public FrameworkElement LeftSide
        {
            get => (FrameworkElement)GetValue(LeftSideProperty);
            set => SetValue(LeftSideProperty, value);
        }
        public static readonly DependencyProperty LeftSideProperty =
            DependencyProperty.Register(nameof(LeftSide), typeof(FrameworkElement), typeof(BannerControl), new PropertyMetadata(null));


        public FrameworkElement RightSide
        {
            get => (FrameworkElement)GetValue(RightSideProperty);
            set => SetValue(RightSideProperty, value);
        }
        public static readonly DependencyProperty RightSideProperty =
            DependencyProperty.Register(nameof(RightSide), typeof(FrameworkElement), typeof(BannerControl), new PropertyMetadata(null));


        public FrameworkElement AdditionalBottomContent
        {
            get => (FrameworkElement)GetValue(AdditionalBottomContentProperty);
            set => SetValue(AdditionalBottomContentProperty, value);
        }
        public static readonly DependencyProperty AdditionalBottomContentProperty =
            DependencyProperty.Register(nameof(AdditionalBottomContent), typeof(FrameworkElement), typeof(BannerControl), new PropertyMetadata(null));
    }
}
