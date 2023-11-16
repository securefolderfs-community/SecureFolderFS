using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SecureFolderFS.AvaloniaUI.UserControls
{
    internal sealed partial class TitleBarControl : UserControl
    {
        public TitleBarControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
