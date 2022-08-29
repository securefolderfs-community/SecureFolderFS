using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Instance.Implementation;
using SecureFolderFS.Core.Security.Loader;

namespace SecureFolderFS.Core.VaultLoader.Routine.Implementation.VaultLoadRoutineSteps
{
    internal sealed class VaultLoadRoutineStep11 : BaseVaultLoadRoutineStep, IVaultLoadRoutineStep11
    {
        public VaultLoadRoutineStep11(VaultInstance vaultInstance, VaultLoadDataModel vaultLoadDataModel)
            : base(vaultInstance, vaultLoadDataModel)
        {
        }

        public IVaultLoadRoutineStep12 ContinueInitialization()
        {
            try
            {
                var securityLoaderFactory = new SecurityLoaderFactory(vaultInstance.VaultVersion);
                var securityLoader = securityLoaderFactory.GetSecurityLoader();

                vaultInstance.Security = securityLoader.LoadSecurity(vaultInstance.BaseVaultConfiguration, vaultLoadDataModel.KeyCryptor, vaultLoadDataModel.MasterKey.CreateCopy());

                return new VaultLoadRoutineStep12(vaultInstance, vaultLoadDataModel);
            }
            finally
            {
                // We have created a copy of MasterKey for ISecurity, so it is safe to dispose this one
                vaultLoadDataModel?.Cleanup();
            }
        }
    }
}
