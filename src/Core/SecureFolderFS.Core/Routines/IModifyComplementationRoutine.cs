using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Core.Routines
{
    public interface IModifyComplementationRoutine : IContractRoutine, IOptionsRoutine
    {
        void SetCredentials(ComplementationCredentials credentials);
    }
}
