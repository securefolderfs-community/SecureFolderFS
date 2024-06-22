using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Inject<INavigationService>(Visibility = "public")]
    [Bindable(true)]
    public sealed partial class SettingsOverlayViewModel : OverlayViewModel
    {
        public static SettingsOverlayViewModel Instance { get; } = new();

        private SettingsOverlayViewModel()
        {
            ServiceProvider = Ioc.Default;
        }
    }
}
