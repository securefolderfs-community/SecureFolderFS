using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Instance.Implementation;
using SecureFolderFS.Core.VaultLoader.Routine.Implementation.VaultLoadRoutineSteps;

namespace SecureFolderFS.Core.VaultLoader.Routine.Implementation
{
    internal sealed class VaultLoadRoutine : IVaultLoadRoutine
    {
        public IVaultLoadRoutineStep1 EstablishRoutine()
        {
            VaultInstance vaultInstance = new VaultInstance();
            VaultLoadDataModel vaultLoadDataModel = new VaultLoadDataModel();

            return new VaultLoadRoutineStep1(vaultInstance, vaultLoadDataModel);
        }
    }
}
