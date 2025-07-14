using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    public sealed partial class StorableTypeOverlayViewModel : BaseDesignationViewModel
    {
        [ObservableProperty] private StorableType _StorableType;

        public StorableTypeOverlayViewModel()
        {
            Title = "Choose an item type";
        }
    }
}
