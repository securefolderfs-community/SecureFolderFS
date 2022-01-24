using SecureFolderFS.Core.VaultCreator.Routine;
using SecureFolderFS.Core.VaultCreator.Routine.Implementation;
using SecureFolderFS.Core.VaultLoader.Routine;
using SecureFolderFS.Core.VaultLoader.Routine.Implementation;

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
        public static IVaultLoadRoutine NewVaultLoadRoutine()
        {
            return new VaultLoadRoutine();
        }

        public static IVaultCreationRoutine NewVaultCreationRoutine()
        {
            return new VaultCreationRoutine();
        }
    }
}
