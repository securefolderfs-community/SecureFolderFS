using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultCreator.Generators.ConfigurationGeneration;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep3 : BaseVaultCreationRoutineStep, IVaultCreationRoutineStep3
    {
        public VaultCreationRoutineStep3(VaultCreationDataModel vaultCreationDataModel)
            : base(vaultCreationDataModel)
        {
        }

        public IVaultCreationRoutineStep4 CreateConfigurationFile(IVaultConfigurationGenerator vaultConfigurationGenerator = null)
        {
            vaultConfigurationGenerator ??= new FromVaultPathVaultConfigurationGenerator(vaultCreationDataModel.FileOperations);

            vaultCreationDataModel.VaultConfigurationStream = vaultConfigurationGenerator.GenerateVaultConfig(vaultCreationDataModel.VaultPath.VaultRootPath, Constants.VAULT_CONFIGURATION_FILENAME);

            return new VaultCreationRoutineStep4(vaultCreationDataModel);
        }
    }
}
