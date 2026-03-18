using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Root;
using SecureFolderFS.Shared.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.InterfaceRoot
{
    public sealed partial class VaultPreviewRootControl : UserControl
    {
        public VaultPreviewViewModel? ViewModel
        {
            get => DataContext.TryCast<VaultPreviewViewModel>();
            set => DataContext = value;
        }

        public VaultPreviewRootControl(VaultPreviewViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
        }
    }
}
