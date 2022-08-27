using SecureFolderFS.Core.DataModels;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep2 : BaseVaultCreationRoutineStep, IVaultCreationRoutineStep2
    {
        public VaultCreationRoutineStep2(VaultCreationDataModel vaultCreationDataModel)
            : base(vaultCreationDataModel)
        {
        }

        public IVaultCreationRoutineStep3 AddFileOperations()
        {
            return new VaultCreationRoutineStep3(vaultCreationDataModel);
        }
    }
}
