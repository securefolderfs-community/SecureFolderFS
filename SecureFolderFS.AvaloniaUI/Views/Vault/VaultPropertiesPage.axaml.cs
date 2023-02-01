using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.UserControls;

namespace SecureFolderFS.AvaloniaUI.Views.Vault
{
    internal sealed partial class VaultPropertiesPage : Page
    {
        public VaultPropertiesPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}