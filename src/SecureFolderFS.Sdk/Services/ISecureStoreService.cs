using System;

namespace SecureFolderFS.Sdk.Services
{
    public interface ISecureStoreService // TODO
    {
        object ProtectContract(IDisposable unlockContract);

        IDisposable UnprotectContract(object protectedContract);
    }
}
