using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels
{
    public sealed partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableObject? _AppContentViewModel;

        public MainViewModel()
        {
        }
    }
}
