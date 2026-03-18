using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.Widgets
{
    public sealed partial class AggregatedDataWidget : UserControl
    {
        public AggregatedDataWidget()
        {
            InitializeComponent();
        }

        public string? TotalRead
        {
            get => (string?)GetValue(TotalReadProperty);
            set => SetValue(TotalReadProperty, value);
        }
        public static readonly DependencyProperty TotalReadProperty =
            DependencyProperty.Register(nameof(TotalRead), typeof(string), typeof(AggregatedDataWidget), new PropertyMetadata(null));

        public string? TotalWrite
        {
            get => (string?)GetValue(TotalWriteProperty);
            set => SetValue(TotalWriteProperty, value);
        }
        public static readonly DependencyProperty TotalWriteProperty =
            DependencyProperty.Register(nameof(TotalWrite), typeof(string), typeof(AggregatedDataWidget), new PropertyMetadata(null));
    }
}
