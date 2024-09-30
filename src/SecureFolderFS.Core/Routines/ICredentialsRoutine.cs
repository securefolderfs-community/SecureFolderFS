using SecureFolderFS.Core.Cryptography.SecureStore;

namespace SecureFolderFS.Core.Routines
{
    // TODO: Needs docs
    public interface ICredentialsRoutine : IFinalizationRoutine
    {
        void SetCredentials(SecretKey passkey);
    }
}
