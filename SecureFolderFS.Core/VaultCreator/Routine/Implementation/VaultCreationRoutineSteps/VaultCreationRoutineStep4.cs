using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultCreator.Generators.KeystoreGeneration;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep4 : IVaultCreationRoutineStep4
    {
        private readonly VaultCreationDataModel _vaultCreationDataModel;

        public VaultCreationRoutineStep4(VaultCreationDataModel vaultCreationDataModel)
        {
            this._vaultCreationDataModel = vaultCreationDataModel;
        }

        public IVaultCreationRoutineStep5 CreateKeystoreFile(IVaultKeystoreGenerator vaultKeystoreGenerator = null)
        {
            vaultKeystoreGenerator ??= new FromVaultPathVaultKeystoreGenerator(_vaultCreationDataModel.FileOperations);
            _vaultCreationDataModel.VaultKeystoreStream = vaultKeystoreGenerator.GenerateVaultKeystore(_vaultCreationDataModel.VaultPath.VaultRootPath, Constants.VAULT_KEYSTORE_FILENAME);

            return new VaultCreationRoutineStep5(_vaultCreationDataModel);
        }
    }
}
