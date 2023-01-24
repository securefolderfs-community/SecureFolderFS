using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SecureFolderFS.AvaloniaUI.Dialogs
{
    internal sealed partial class VaultWizardDialog : UserControl
    {
        public VaultWizardDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}