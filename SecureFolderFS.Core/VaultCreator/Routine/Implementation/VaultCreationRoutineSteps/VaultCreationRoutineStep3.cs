using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Discoverers;
using SecureFolderFS.Core.VaultCreator.Generators;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep3 : BaseVaultCreationRoutineStep, IVaultCreationRoutineStep3
    {
        public VaultCreationRoutineStep3(VaultCreationDataModel vaultCreationDataModel)
            : base(vaultCreationDataModel)
        {
        }

        public IVaultCreationRoutineStep4 CreateConfigurationFile(IVaultConfigurationDiscoverer vaultConfigurationDiscoverer = null)
        {
            vaultConfigurationDiscoverer ??= new FromVaultPathVaultConfigurationGenerator();

            vaultCreationDataModel.VaultConfigurationStream = vaultConfigurationDiscoverer.DiscoverVaultConfig(vaultCreationDataModel.VaultPath.VaultRootPath, Constants.VAULT_CONFIGURATION_FILENAME);

            return new VaultCreationRoutineStep4(vaultCreationDataModel);
        }
    }
}
