using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Discoverers;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Instance.Implementation;
using SecureFolderFS.Core.VaultLoader.Discoverers;

namespace SecureFolderFS.Core.VaultLoader.Routine.Implementation.VaultLoadRoutineSteps
{
    internal sealed class VaultLoadRoutineStep3 : BaseVaultLoadRoutineStep, IVaultLoadRoutineStep3
    {
        public VaultLoadRoutineStep3(VaultInstance vaultInstance, VaultLoadDataModel vaultLoadDataModel)
            : base(vaultInstance, vaultLoadDataModel)
        {
        }

        public IVaultLoadRoutineStep4 FindConfigurationFile(bool useExternalDiscoverer = false, IVaultConfigurationDiscoverer vaultConfigurationDiscoverer = null)
        {
            vaultLoadDataModel.VaultConfigurationStream = VaultHelpers.FindVaultFile(
                () => new FromVaultPathVaultConfigurationDiscoverer(vaultInstance.FileOperations),
                vaultFileDiscoverer => vaultFileDiscoverer.DiscoverVaultConfig(vaultInstance.VaultPath.VaultRootPath, Constants.VAULT_CONFIGURATION_FILENAME),
                useExternalDiscoverer,
                vaultConfigurationDiscoverer);

            return new VaultLoadRoutineStep4(vaultInstance, vaultLoadDataModel);
        }
    }
}
