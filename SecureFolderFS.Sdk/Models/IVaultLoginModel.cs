using SecureFolderFS.Shared.Utils;
using System;

namespace SecureFolderFS.Sdk.Models
{
    public interface IVaultLoginModel : IAsyncInitialize, IDisposable
    {
        IVaultModel VaultModel { get; }
        
        IVaultWatcherModel VaultWatcher { get; }

        event EventHandler<IResult<VaultLoginStateType>>? StateChanged;
    }

    public enum VaultLoginStateType
    {
        AwaitingCredentials = 0,
        AwaitingTwoFactorAuth = 1,
        VaultError = 2,
    }
}
