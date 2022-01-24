using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Security.EncryptionAlgorithm.Builder;
using SecureFolderFS.Core.Security.KeyCrypt;
using SecureFolderFS.Core.VaultDataStore;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep6 : IVaultCreationRoutineStep6
    {
        private readonly VaultCreationDataModel _vaultCreationDataModel;

        public VaultCreationRoutineStep6(VaultCreationDataModel vaultCreationDataModel)
        {
            this._vaultCreationDataModel = vaultCreationDataModel;
        }

        public IVaultCreationRoutineStep7 AddEncryptionAlgorithmBuilder(IEncryptionAlgorithmBuilder encryptionAlgorithmBuilder = null)
        {
            encryptionAlgorithmBuilder ??= EncryptionAlgorithmBuilder.GetBuilder().DoFinal();

            var keyCryptFactory = new KeyCryptFactory(new VaultVersion(VaultVersion.HIGHEST_VERSION), encryptionAlgorithmBuilder);
            _vaultCreationDataModel.KeyCryptor = keyCryptFactory.GetKeyCryptor();

            return new VaultCreationRoutineStep7(_vaultCreationDataModel);
        }
    }
}
