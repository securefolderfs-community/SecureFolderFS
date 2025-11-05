using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SecureFolderFS.Uno.UserControls
{
    public sealed partial class RecoveryControl : UserControl
    {
        public RecoveryControl()
        {
            InitializeComponent();
        }

        public string? RecoveryKey
        {
            get => (string?)GetValue(RecoveryKeyProperty);
            set => SetValue(RecoveryKeyProperty, value);
        }
        public static readonly DependencyProperty RecoveryKeyProperty =
            DependencyProperty.Register(nameof(RecoveryKey), typeof(string), typeof(RecoveryControl), new PropertyMetadata(null));

        public ICommand? PasteRecoveryKeyCommand
        {
            get => (ICommand?)GetValue(PasteRecoveryKeyCommandProperty);
            set => SetValue(PasteRecoveryKeyCommandProperty, value);
        }
        public static readonly DependencyProperty PasteRecoveryKeyCommandProperty =
            DependencyProperty.Register(nameof(PasteRecoveryKeyCommand), typeof(ICommand), typeof(RecoveryControl), new PropertyMetadata(null));
    }
}
