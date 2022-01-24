using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultCreator.Generators.ConfigurationGeneration;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep3 : IVaultCreationRoutineStep3
    {
        private readonly VaultCreationDataModel _vaultCreationDataModel;

        public VaultCreationRoutineStep3(VaultCreationDataModel vaultCreationDataModel)
        {
            this._vaultCreationDataModel = vaultCreationDataModel;
        }

        public IVaultCreationRoutineStep4 CreateConfigurationFile(IVaultConfigurationGenerator vaultConfigurationGenerator = null)
        {
            vaultConfigurationGenerator ??= new FromVaultPathVaultConfigurationGenerator(_vaultCreationDataModel.FileOperations);

            _vaultCreationDataModel.VaultConfigurationStream = vaultConfigurationGenerator.GenerateVaultConfig(_vaultCreationDataModel.VaultPath.VaultRootPath, Constants.VAULT_CONFIGURATION_FILENAME);

            return new VaultCreationRoutineStep4(_vaultCreationDataModel);
        }
    }
}
