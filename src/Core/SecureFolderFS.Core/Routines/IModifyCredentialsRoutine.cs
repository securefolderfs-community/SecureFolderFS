using SecureFolderFS.Shared.ComponentModel;
using System.Threading;

namespace SecureFolderFS.Core.Routines
{
    public interface IModifyCredentialsRoutine : ICredentialsRoutine, IContractRoutine, IOptionsRoutine
    {
        void SetCredentials(IKeyUsage oldPasskey, IKeyUsage newPasskey, CancellationToken cancellationToken = default);
    }
}
