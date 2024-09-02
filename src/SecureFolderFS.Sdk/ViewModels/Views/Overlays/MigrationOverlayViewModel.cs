using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    public sealed partial class MigrationOverlayViewModel : OverlayViewModel
    {
        [ObservableProperty] private MigrationViewModel _MigrationViewModel;

        public MigrationOverlayViewModel(MigrationViewModel migrationViewModel)
        {
            // For simplicity's sake there's no inheritance of MigrationViewModel,
            // and appropriate migrators are chosen based solely on vault version
            _MigrationViewModel = migrationViewModel;
        }
    }
}
