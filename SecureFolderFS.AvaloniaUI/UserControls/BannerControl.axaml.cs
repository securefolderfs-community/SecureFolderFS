using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SecureFolderFS.AvaloniaUI.UserControls
{
    public sealed partial class BannerControl : UserControl
    {
        public BannerControl()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public Control LeftSide
        {
            get => GetValue(LeftSideProperty);
            set => SetValue(LeftSideProperty, value);
        }
        public static readonly StyledProperty<Control> LeftSideProperty =
            AvaloniaProperty.Register<BannerControl, Control>(nameof(LeftSide));

        public Control RightSide
        {
            get => GetValue(RightSideProperty);
            set => SetValue(RightSideProperty, value);
        }
        public static readonly StyledProperty<Control> RightSideProperty =
            AvaloniaProperty.Register<BannerControl, Control>(nameof(RightSide));

        public Control AdditionalBottomContent
        {
            get => GetValue(AdditionalBottomContentProperty);
            set => SetValue(AdditionalBottomContentProperty, value);
        }
        public static readonly StyledProperty<Control> AdditionalBottomContentProperty =
            AvaloniaProperty.Register<BannerControl, Control>(nameof(AdditionalBottomContent));
    }
}