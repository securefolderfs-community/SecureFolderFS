using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    public sealed partial class CustomizationOverlayViewModel : OverlayViewModel
    {
        [ObservableProperty] private VaultViewModel _VaultViewModel;

        public CustomizationOverlayViewModel(VaultViewModel vaultViewModel)
        {
            VaultViewModel = vaultViewModel;
        }
    }
}
