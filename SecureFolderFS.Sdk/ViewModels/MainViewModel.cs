using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Sdk.ViewModels
{
    public sealed partial class MainViewModel : ObservableObject
    {
        public INavigationService HostNavigationService { get; } = Ioc.Default.GetRequiredService<INavigationService>();

        [ObservableProperty]
        private INotifyPropertyChanged? _AppContentViewModel;
    }
}
