using System;
using System.Collections.Generic;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels;

namespace SecureFolderFS.Sdk.Services.Settings
{
    public interface ISettingsService : ISettingsModel
    {
        [Obsolete("This property has been deprecated. Use IVaultsSettingsService.SavedVaults instead.")]
        Dictionary<VaultIdModel, VaultViewModel> SavedVaults { get; set; }
    }
}
