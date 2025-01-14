using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    public sealed partial class RenameOverlayViewModel : ObservableObject, IViewable
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private string? _Message;
        [ObservableProperty] private string? _NewName;

        public RenameOverlayViewModel(string? title)
        {
            Title = title;
        }
    }
}
