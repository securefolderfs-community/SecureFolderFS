using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Instance.Implementation;

namespace SecureFolderFS.Core.VaultLoader.Routine.Implementation.VaultLoadRoutineSteps
{
    internal abstract class BaseVaultLoadRoutineStep
    {
        protected readonly VaultInstance vaultInstance;

        protected readonly VaultLoadDataModel vaultLoadDataModel;

        protected BaseVaultLoadRoutineStep(VaultInstance vaultInstance, VaultLoadDataModel vaultLoadDataModel)
        {
            vaultInstance = vaultInstance;
            vaultLoadDataModel = vaultLoadDataModel;
        }
    }
}
