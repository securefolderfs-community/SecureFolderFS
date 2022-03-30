using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

#nullable enable

namespace SecureFolderFS.WinUI.UserControls.ActionBlocks
{
    public sealed partial class ActionBlockContentControl : UserControl
    {
        public ActionBlockContentControl()
        {
            this.InitializeComponent();
        }

        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(IconElement), typeof(ActionBlockContentControl), new PropertyMetadata(null));


        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ActionBlockContentControl), new PropertyMetadata(null));


        public string? Description
        {
            get => (string?)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(ActionBlockContentControl), new PropertyMetadata(null));


        public FrameworkElement CustomDescription
        {
            get => (FrameworkElement)GetValue(CustomDescriptionProperty);
            set => SetValue(CustomDescriptionProperty, value);
        }
        public static readonly DependencyProperty CustomDescriptionProperty =
            DependencyProperty.Register(nameof(CustomDescription), typeof(FrameworkElement), typeof(ActionBlockContentControl), new PropertyMetadata(null));


        public FrameworkElement ActionElement
        {
            get => (FrameworkElement)GetValue(ActionElementProperty);
            set => SetValue(ActionElementProperty, value);
        }
        public static readonly DependencyProperty ActionElementProperty =
            DependencyProperty.Register(nameof(ActionElement), typeof(FrameworkElement), typeof(ActionBlockContentControl), new PropertyMetadata(null));


        public FrameworkElement AdditionalActionElement
        {
            get => (FrameworkElement)GetValue(AdditionalActionElementProperty);
            set => SetValue(AdditionalActionElementProperty, value);
        }
        public static readonly DependencyProperty AdditionalActionElementProperty =
            DependencyProperty.Register(nameof(AdditionalActionElement), typeof(FrameworkElement), typeof(ActionBlockContentControl), new PropertyMetadata(null));
    }
}
