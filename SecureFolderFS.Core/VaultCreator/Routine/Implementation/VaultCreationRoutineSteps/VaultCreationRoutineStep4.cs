using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Discoverers;
using SecureFolderFS.Core.VaultCreator.Generators;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep4 : BaseVaultCreationRoutineStep, IVaultCreationRoutineStep4
    {
        public VaultCreationRoutineStep4(VaultCreationDataModel vaultCreationDataModel)
            : base(vaultCreationDataModel)
        {
        }

        public IVaultCreationRoutineStep5 CreateKeystoreFile(IVaultKeystoreDiscoverer vaultKeystoreDiscoverer = null)
        {
            vaultKeystoreDiscoverer ??= new FromVaultPathVaultKeystoreGenerator();
            vaultCreationDataModel.VaultKeystoreStream = vaultKeystoreDiscoverer.DiscoverVaultKeystore(vaultCreationDataModel.VaultPath.VaultRootPath, Constants.VAULT_KEYSTORE_FILENAME);

            return new VaultCreationRoutineStep5(vaultCreationDataModel);
        }
    }
}
