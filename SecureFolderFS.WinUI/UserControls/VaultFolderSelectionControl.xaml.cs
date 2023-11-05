using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls
{
    public sealed partial class VaultFolderSelectionControl : UserControl
    {
        public VaultFolderSelectionControl()
        {
            InitializeComponent();
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(VaultFolderSelectionControl), new PropertyMetadata(null));

        public ICommand OpenCommand
        {
            get => (ICommand)GetValue(OpenCommandProperty);
            set => SetValue(OpenCommandProperty, value);
        }
        public static readonly DependencyProperty OpenCommandProperty =
            DependencyProperty.Register(nameof(OpenCommand), typeof(ICommand), typeof(VaultFolderSelectionControl), new PropertyMetadata(null));
    }
}
