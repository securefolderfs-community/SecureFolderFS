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

        /// <summary>
        /// Set to true when the user requests an account key reset (forgot passphrase flow)
        /// instead of providing a passphrase. The caller should handle the reset API call.
        /// </summary>
        public bool ResetRequested { get; set; }

        public DeviceSetupOverlayViewModel()
        {
            Title = "Device Setup Required";
            PrimaryText = "Continue";
            CanContinue = true;
            CanCancel = true;
        }
    }
}