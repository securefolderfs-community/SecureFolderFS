using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    public sealed partial class StorableTypeOverlayViewModel : BaseDesignationViewModel
    {
        [ObservableProperty] private StorableType _StorableType;
        [ObservableProperty] private bool _IncludeGallery;
        [ObservableProperty] private string? _SelectedOption;

        public StorableTypeOverlayViewModel()
        {
            Title = "Choose an item type";
        }
    }
}
