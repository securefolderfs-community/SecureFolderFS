using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.ViewModels;

namespace SecureFolderFS.Backend.Services.Settings
{
    public interface ISettingsService : IBaseSettingsService
    {
        IGeneralSettingsService GeneralSettingsService { get; }

        IPreferencesSettingsService PreferencesSettingsService { get; }

        ISecuritySettingsService SecuritySettingsService { get; }

        Dictionary<VaultIdModel, VaultViewModel> SavedVaults { get; set; }

        TSharingContext GetSharingContext<TSharingContext>() where TSharingContext : class;
    }
}
