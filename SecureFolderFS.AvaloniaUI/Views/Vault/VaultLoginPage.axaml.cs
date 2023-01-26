using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SecureFolderFS.AvaloniaUI.Views.Vault
{
    public sealed partial class VaultLoginPage : UserControl
    {
        public VaultLoginPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}