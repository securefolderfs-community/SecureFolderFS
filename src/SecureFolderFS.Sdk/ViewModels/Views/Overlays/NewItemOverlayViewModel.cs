using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    public sealed partial class NewItemOverlayViewModel : OverlayViewModel, IViewable
    {
        [ObservableProperty] private string? _Message;
        [ObservableProperty] private string? _ItemName;

        public NewItemOverlayViewModel()
        {
            Title = "Set new name";
            Message = "A new item will be created with the provided name.";
        }
    }
}
