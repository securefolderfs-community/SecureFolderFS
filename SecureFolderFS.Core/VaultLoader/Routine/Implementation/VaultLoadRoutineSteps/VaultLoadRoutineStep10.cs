using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.Instance.Implementation;

namespace SecureFolderFS.Core.VaultLoader.Routine.Implementation.VaultLoadRoutineSteps
{
    internal sealed class VaultLoadRoutineStep10 : BaseVaultLoadRoutineStep, IVaultLoadRoutineStep10
    {
        public VaultLoadRoutineStep10(VaultInstance vaultInstance, VaultLoadDataModel vaultLoadDataModel)
            : base(vaultInstance, vaultLoadDataModel)
        {
        }

        public IVaultLoadRoutineStep11 VerifyVaultConfiguration()
        {
            try
            {
                if (!vaultInstance.BaseVaultConfiguration.Verify(vaultLoadDataModel.KeyCryptor, vaultLoadDataModel.MasterKey))
                {
                    throw new UnauthenticVaultConfigurationException();
                }

                return new VaultLoadRoutineStep11(vaultInstance, vaultLoadDataModel);
            }
            catch
            {
                vaultLoadDataModel?.Cleanup();
                throw;
            }
        }
    }
}
