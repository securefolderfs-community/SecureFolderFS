using System.Collections.Generic;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services.Settings
{
    /// <summary>
    /// Contains properties to be stored secretly.
    /// This interface does not guarantee security of data and therefore should not be used to store sensitive data.
    /// </summary>
    public interface ISecretSettingsService : ISettingsModel
    {
        Dictionary<VaultIdModel, VaultModelDeprecated> SavedVaultModels { get; set; }
    }
}
