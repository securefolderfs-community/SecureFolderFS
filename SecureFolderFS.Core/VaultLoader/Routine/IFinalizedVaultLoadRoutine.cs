using SecureFolderFS.Core.Instance;

namespace SecureFolderFS.Core.VaultLoader.Routine
{
    public interface IFinalizedVaultLoadRoutine
    {
        IOptionalVaultLoadRoutine ContinueWithOptionalRoutine();

        IVaultInstance Deploy();
    }
}
