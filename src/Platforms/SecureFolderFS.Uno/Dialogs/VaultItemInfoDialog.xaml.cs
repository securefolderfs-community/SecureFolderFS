using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Utils;
using SecureFolderFS.Uno.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class VaultItemInfoDialog : ContentDialog, IOverlayControl
    {
        public VaultItemInfoOverlayViewModel? ViewModel
        {
            get => DataContext.TryCast<VaultItemInfoOverlayViewModel>();
            set => DataContext = value;
        }

        public VaultItemInfoDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => (await base.ShowAsync()).ParseOverlayOption();

        /// <inheritdoc/>
        public void SetView(IViewable viewable) => ViewModel = (VaultItemInfoOverlayViewModel)viewable;

        /// <inheritdoc/>
        public Task HideAsync()
        {
            Hide();
            return Task.CompletedTask;
        }
    }
}
