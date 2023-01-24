using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SecureFolderFS.AvaloniaUI.UserControls.InterfaceRoot
{
    public sealed partial class MainWindowRootControl : UserControl
    {
        public MainWindowRootControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}