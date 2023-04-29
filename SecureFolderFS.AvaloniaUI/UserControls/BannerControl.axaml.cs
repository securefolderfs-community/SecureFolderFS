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

        public IControl LeftSide
        {
            get => GetValue(LeftSideProperty);
            set => SetValue(LeftSideProperty, value);
        }
        public static readonly StyledProperty<IControl> LeftSideProperty =
            AvaloniaProperty.Register<BannerControl, IControl>(nameof(LeftSide));

        public IControl RightSide
        {
            get => GetValue(RightSideProperty);
            set => SetValue(RightSideProperty, value);
        }
        public static readonly StyledProperty<IControl> RightSideProperty =
            AvaloniaProperty.Register<BannerControl, IControl>(nameof(RightSide));

        public IControl AdditionalBottomContent
        {
            get => GetValue(AdditionalBottomContentProperty);
            set => SetValue(AdditionalBottomContentProperty, value);
        }
        public static readonly StyledProperty<IControl> AdditionalBottomContentProperty =
            AvaloniaProperty.Register<BannerControl, IControl>(nameof(AdditionalBottomContent));
    }
}