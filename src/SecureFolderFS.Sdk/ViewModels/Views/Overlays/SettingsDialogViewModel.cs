using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Inject<INavigationService>(Visibility = "public")]
    public sealed partial class SettingsDialogViewModel : DialogViewModel
    {
        public static SettingsDialogViewModel Instance { get; } = new();

        private SettingsDialogViewModel()
        {
            ServiceProvider = Ioc.Default;
        }
    }
}
