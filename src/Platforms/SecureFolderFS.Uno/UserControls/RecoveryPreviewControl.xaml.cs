using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls
{
    public sealed partial class RecoveryPreviewControl : UserControl
    {
        public RecoveryPreviewControl()
        {
            InitializeComponent();
        }

        public string? RecoveryKey
        {
            get => (string?)GetValue(RecoveryKeyProperty);
            set => SetValue(RecoveryKeyProperty, value);
        }
        public static readonly DependencyProperty RecoveryKeyProperty =
            DependencyProperty.Register(nameof(RecoveryKey), typeof(string), typeof(RecoveryPreviewControl), new PropertyMetadata(null));

        public ICommand? ExportCommand
        {
            get => (ICommand?)GetValue(ExportCommandProperty);
            set => SetValue(ExportCommandProperty, value);
        }
        public static readonly DependencyProperty ExportCommandProperty =
            DependencyProperty.Register(nameof(ExportCommand), typeof(ICommand), typeof(RecoveryPreviewControl), new PropertyMetadata(null));

        public ICommand? RevealCommand
        {
            get => (ICommand?)GetValue(RevealCommandProperty);
            set => SetValue(RevealCommandProperty, value);
        }
        public static readonly DependencyProperty RevealCommandProperty =
            DependencyProperty.Register(nameof(RevealCommand), typeof(ICommand), typeof(RecoveryPreviewControl), new PropertyMetadata(null));

        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(RecoveryPreviewControl), new PropertyMetadata(false));
    }
}
