using SecureFolderFS.Core.DataModels;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep12 : BaseVaultCreationRoutineStep, IVaultCreationRoutineStep12
    {
        public VaultCreationRoutineStep12(VaultCreationDataModel vaultCreationDataModel)
            : base(vaultCreationDataModel)
        {
        }

        public IFinalizedVaultCreationRoutine Finalize()
        {
            vaultCreationDataModel.Cleanup();
            return new FinalizedVaultCreationRoutine();
        }
    }
}
