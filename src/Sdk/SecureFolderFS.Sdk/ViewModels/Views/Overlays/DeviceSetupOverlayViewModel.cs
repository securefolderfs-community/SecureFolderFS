using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    /// <summary>
    /// Overlay view model for the App Platform device setup dialog.
    /// Prompts the user for their Account Key passphrase to bootstrap a new device.
    /// </summary>
    [Bindable(true)]
    public sealed partial class DeviceSetupOverlayViewModel : OverlayViewModel
    {
        [ObservableProperty] private string? _Passphrase;
        [ObservableProperty] private string? _ErrorMessage;

        public DeviceSetupOverlayViewModel()
        {
            Title = "Device Setup Required";
            PrimaryText = "Continue";
            CanContinue = true;
            CanCancel = true;
        }
    }
}