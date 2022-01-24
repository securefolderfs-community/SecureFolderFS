using SecureFolderFS.Core.DataModels;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep12 : IVaultCreationRoutineStep12
    {
        private readonly VaultCreationDataModel _vaultCreationDataModel;

        public VaultCreationRoutineStep12(VaultCreationDataModel vaultCreationDataModel)
        {
            this._vaultCreationDataModel = vaultCreationDataModel;
        }

        public IFinalizedVaultCreationRoutine Finish()
        {
            _vaultCreationDataModel.Cleanup();
            return new FinalizedVaultCreationRoutine();
        }
    }
}
