using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation
{
    internal sealed class VaultCreationRoutine : IVaultCreationRoutine
    {
        public IVaultCreationRoutineStep1 EstablishRoutine()
        {
            VaultCreationDataModel vaultCreationDataModel = new VaultCreationDataModel();

            return new VaultCreationRoutineStep1(vaultCreationDataModel);
        }
    }
}
