using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels
{
    public sealed partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private INotifyPropertyChanged? _AppContentViewModel;
    }
}
