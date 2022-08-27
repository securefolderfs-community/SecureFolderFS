using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Instance.Implementation;

namespace SecureFolderFS.Core.VaultLoader.Routine.Implementation.VaultLoadRoutineSteps
{
    internal sealed class VaultLoadRoutineStep2 : BaseVaultLoadRoutineStep, IVaultLoadRoutineStep2
    {
        public VaultLoadRoutineStep2(VaultInstance vaultInstance, VaultLoadDataModel vaultLoadDataModel)
            : base(vaultInstance, vaultLoadDataModel)
        {
        }

        public IVaultLoadRoutineStep3 AddFileOperations()
        {
            return new VaultLoadRoutineStep3(vaultInstance, vaultLoadDataModel);
        }
    }
}
