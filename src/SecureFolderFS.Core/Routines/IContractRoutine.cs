using System;

namespace SecureFolderFS.Core.Routines
{
    public interface IContractRoutine
    {
        void SetUnlockContract(IDisposable unlockContract);
    }
}
