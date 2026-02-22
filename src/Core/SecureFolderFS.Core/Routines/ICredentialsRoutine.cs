using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Core.Routines
{
    // TODO: Needs docs
    public interface ICredentialsRoutine : IFinalizationRoutine
    {
        void SetCredentials(IKeyUsage passkey);
    }
}
