using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Instance.Implementation;

namespace SecureFolderFS.Core.VaultLoader.Routine.Implementation.VaultLoadRoutineSteps
{
    internal sealed class VaultLoadRoutineStep12 : BaseVaultLoadRoutineStep, IVaultLoadRoutineStep12
    {
        public VaultLoadRoutineStep12(VaultInstance vaultInstance, VaultLoadDataModel vaultLoadDataModel)
            : base(vaultInstance, vaultLoadDataModel)
        {
        }

        public IFinalizedVaultLoadRoutine Finalize()
        {
            return new FinalizedVaultLoadRoutine(vaultInstance, vaultLoadDataModel);
        }
    }
}
