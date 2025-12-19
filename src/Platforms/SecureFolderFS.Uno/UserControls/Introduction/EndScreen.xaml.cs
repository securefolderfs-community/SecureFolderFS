using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.Introduction
{
    public sealed partial class EndScreen : UserControl
    {
        public EndScreen()
        {
            InitializeComponent();
        }

        public ICommand? CloseCommand
        {
            get => (ICommand?)GetValue(CloseCommandProperty);
            set => SetValue(CloseCommandProperty, value);
        }
        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), typeof(EndScreen), new PropertyMetadata(null));
    }
}
