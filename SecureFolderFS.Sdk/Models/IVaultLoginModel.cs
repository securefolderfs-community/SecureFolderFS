using SecureFolderFS.Shared.Utils;
using System;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Sdk.Models
{
    // TODO: Needs docs
    public interface IVaultLoginModel : IAsyncInitialize, IDisposable
    {
        IVaultModel VaultModel { get; }
        
        IVaultWatcherModel VaultWatcher { get; }

        event EventHandler<IResult<VaultLoginStateType>>? StateChanged;
    }
}
