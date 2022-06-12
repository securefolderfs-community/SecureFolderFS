using System.Collections.Generic;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels;

namespace SecureFolderFS.Sdk.Services.Settings
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
