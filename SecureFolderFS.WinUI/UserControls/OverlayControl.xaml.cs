using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls
{
    public sealed partial class OverlayControl : UserControl
    {
        public OverlayControl()
        {
            this.InitializeComponent();
        }

        public object? OverlayContent
        {
            get => (object?)GetValue(OverlayContentProperty);
            set => SetValue(OverlayContentProperty, value);
        }
        public static readonly DependencyProperty OverlayContentProperty =
            DependencyProperty.Register(nameof(OverlayContent), typeof(object), typeof(OverlayControl), new PropertyMetadata(null));
    }
}
