using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Instance.Implementation;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.VaultLoader.Routine.Implementation.VaultLoadRoutineSteps
{
    internal sealed class VaultLoadRoutineStep8 : BaseVaultLoadRoutineStep, IVaultLoadRoutineStep8
    {
        public VaultLoadRoutineStep8(VaultInstance vaultInstance, VaultLoadDataModel vaultLoadDataModel)
            : base(vaultInstance, vaultLoadDataModel)
        {
        }

        public IVaultLoadRoutineStep9 DeriveMasterKeyFromPassword(IPassword password)
        {
            vaultLoadDataModel.MasterKey = vaultLoadDataModel.MasterKeyDerivation.DeriveMasterKey(password, vaultLoadDataModel.BaseVaultKeystore, vaultLoadDataModel.KeyCryptor);

            return new VaultLoadRoutineStep9(vaultInstance, vaultLoadDataModel);
        }
    }
}
