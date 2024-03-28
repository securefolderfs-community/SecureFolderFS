using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// A model used for monitoring vault contents. 
    /// </summary>
    public interface IVaultWatcherModel : IAsyncInitialize, INotifyStateChanged, IDisposable
    {
        /// <summary>
        /// Gets the vault folder that's being watched.
        /// </summary>
        IFolder VaultFolder { get; }
    }
}
