using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.Services.VaultPersistence
{
    /// <summary>
    /// A service to manage widgets of saved vaults.
    /// </summary>
    public interface IVaultWidgets : IPersistable
    {
        /// <summary>
        /// Gets widget context identified by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id associated with a context.</param>
        WidgetsDataModel GetWidgetsContextForId(string id);
    }
}
