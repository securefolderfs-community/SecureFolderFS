using SecureFolderFS.Shared.Utils;
using System;

namespace SecureFolderFS.Sdk.Models
{
    public interface IVaultLoginModel : IAsyncInitialize, IDisposable
    {
        IVaultModel VaultModel { get; }
        
        IVaultWatcherModel VaultWatcher { get; }

        event EventHandler<IVaultStrategyModel>? StrategyChanged;
    }
}
