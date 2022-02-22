using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultCreator.Generators.KeystoreGeneration;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep4 : BaseVaultCreationRoutineStep, IVaultCreationRoutineStep4
    {
        public VaultCreationRoutineStep4(VaultCreationDataModel vaultCreationDataModel)
            : base(vaultCreationDataModel)
        {
        }

        public IVaultCreationRoutineStep5 CreateKeystoreFile(IVaultKeystoreGenerator vaultKeystoreGenerator = null)
        {
            vaultKeystoreGenerator ??= new FromVaultPathVaultKeystoreGenerator(vaultCreationDataModel.FileOperations);
            vaultCreationDataModel.VaultKeystoreStream = vaultKeystoreGenerator.GenerateVaultKeystore(vaultCreationDataModel.VaultPath.VaultRootPath, Constants.VAULT_KEYSTORE_FILENAME);

            return new VaultCreationRoutineStep5(vaultCreationDataModel);
        }
    }
}
