using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Sdk.ViewModels.Dialogs
{
    public sealed class SettingsDialogViewModel : DialogViewModel
    {
        public static SettingsDialogViewModel Instance { get; } = new();

        public INavigationService NavigationService { get; } = Ioc.Default.GetRequiredService<INavigationService>();

        private SettingsDialogViewModel()
        {
        }
    }
}
