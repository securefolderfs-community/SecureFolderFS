using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Instance.Implementation;

namespace SecureFolderFS.Core.VaultLoader.Routine.Implementation.VaultLoadRoutineSteps
{
    internal sealed class VaultLoadRoutineStep9 : BaseVaultLoadRoutineStep, IVaultLoadRoutineStep9
    {
        public VaultLoadRoutineStep9(VaultInstance vaultInstance, VaultLoadDataModel vaultLoadDataModel)
            : base(vaultInstance, vaultLoadDataModel)
        {
        }

        public IVaultLoadRoutineStep10 ContinueInitializationWithMasterKey()
        {
            return new VaultLoadRoutineStep10(vaultInstance, vaultLoadDataModel);
        }
    }
}
