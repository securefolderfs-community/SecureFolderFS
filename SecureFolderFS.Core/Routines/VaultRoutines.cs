using SecureFolderFS.Core.Routines.CreationRoutines;
using SecureFolderFS.Core.Routines.UnlockRoutines;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Shared.Utils;
using System.IO;

namespace SecureFolderFS.Core.Routines
{
    /// <summary>
    /// Provides implementation for receiving vault routines.
    /// <br/>
    /// <br/>
    /// This SDK is exposed.
    /// </summary>
    public static class VaultRoutines
    {
        public static IUnlockRoutine NewUnlockRoutine()
        {
            return new UnlockRoutine();
        }

        public static ICreationRoutine NewCreationRoutine()
        {
            return new CreationRoutine();
        }

        public static IAsyncValidator<Stream> NewVersionValidator(IAsyncSerializer<Stream> serializer)
        {
            return new VersionValidator(serializer);
        }
    }
}
