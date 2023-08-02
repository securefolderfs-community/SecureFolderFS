using SecureFolderFS.Shared.Utilities;
using System;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Sdk.Models
{
    // TODO: Needs docs
    public interface IVaultLoginModel : IAsyncInitialize, IDisposable
    {
        /// <summary>
        /// Gets associated <see cref="IVaultModel"/> with this model.
        /// </summary>
        IVaultModel VaultModel { get; }
        
        IVaultWatcherModel VaultWatcher { get; }

        event EventHandler<IResult<VaultLoginStateType>>? StateChanged;
    }
}
