using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// A model used to monitor system events.
    /// </summary>
    public interface ISystemMonitorModel : IAsyncInitialize, IDisposable
    {
        /// <summary>
        /// Gets the collection model of vaults associated with this instance.
        /// </summary>
        IVaultCollectionModel VaultCollectionModel { get; }
    }
}
