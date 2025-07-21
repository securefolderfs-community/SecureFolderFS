using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    public partial class MessageOverlayViewModel : OverlayViewModel
    {
        /// <summary>
        /// Gets or sets the primary message to display in the overlay.
        /// </summary>
        [ObservableProperty] private string? _Message;
    }
}