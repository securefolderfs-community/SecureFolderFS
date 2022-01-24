using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Instance.Implementation;
using SecureFolderFS.Core.Security.EncryptionAlgorithm.Builder;
using SecureFolderFS.Core.Security.KeyCrypt;

namespace SecureFolderFS.Core.VaultLoader.Routine.Implementation.VaultLoadRoutineSteps
{
    internal sealed class VaultLoadRoutineStep7 : BaseVaultLoadRoutineStep, IVaultLoadRoutineStep7
    {
        public VaultLoadRoutineStep7(VaultInstance vaultInstance, VaultLoadDataModel vaultLoadDataModel)
            : base(vaultInstance, vaultLoadDataModel)
        {
        }

        public IVaultLoadRoutineStep8 AddEncryptionAlgorithmBuilder(IEncryptionAlgorithmBuilder encryptionAlgorithmBuilder = null)
        {
            encryptionAlgorithmBuilder ??= EncryptionAlgorithmBuilder.GetBuilder().DoFinal();

            var keyCryptFactory = new KeyCryptFactory(vaultInstance.VaultVersion, encryptionAlgorithmBuilder);
            vaultLoadDataModel.KeyCryptor = keyCryptFactory.GetKeyCryptor();

            return new VaultLoadRoutineStep8(vaultInstance, vaultLoadDataModel);
        }
    }
}
