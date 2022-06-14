using System.Collections.Generic;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels;

namespace SecureFolderFS.Sdk.Services.Settings
{
    public interface ISettingsService : IBaseSettingsService
    {
        Dictionary<VaultIdModel, VaultViewModel> SavedVaults { get; set; }
    }
}
