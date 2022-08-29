using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Security.EncryptionAlgorithm.Builder;
using SecureFolderFS.Core.Security.Cipher;
using SecureFolderFS.Core.VaultDataStore;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep6 : BaseVaultCreationRoutineStep, IVaultCreationRoutineStep6
    {
        public VaultCreationRoutineStep6(VaultCreationDataModel vaultCreationDataModel)
            : base(vaultCreationDataModel)
        {
        }

        public IVaultCreationRoutineStep7 AddEncryptionAlgorithmBuilder(IEncryptionAlgorithmBuilder encryptionAlgorithmBuilder = null)
        {
            encryptionAlgorithmBuilder ??= EncryptionAlgorithmBuilder.GetBuilder().DoFinal();

            var keyCryptFactory = new KeyCryptFactory(new VaultVersion(VaultVersion.HIGHEST_VERSION), encryptionAlgorithmBuilder);
            vaultCreationDataModel.KeyCryptor = keyCryptFactory.GetKeyCryptor();

            return new VaultCreationRoutineStep7(vaultCreationDataModel);
        }
    }
}
