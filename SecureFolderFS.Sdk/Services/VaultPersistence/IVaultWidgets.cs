using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Shared.Utils;
using System.Collections.Generic;

namespace SecureFolderFS.Sdk.Services.VaultPersistence
{
    /// <summary>
    /// A service to manage widgets of saved vaults.
    /// </summary>
    public interface IVaultWidgets : IPersistable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vaultId"></param>
        /// <returns></returns>
        ICollection<WidgetDataModel>? GetForVault(string vaultId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vaultId"></param>
        /// <param name="widgets"></param>
        /// <returns></returns>
        bool SetForVault(string vaultId, ICollection<WidgetDataModel>? widgets);
    }
}
