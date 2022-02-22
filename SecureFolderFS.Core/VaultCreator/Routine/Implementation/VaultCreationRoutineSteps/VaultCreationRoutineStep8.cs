using SecureFolderFS.Core.DataModels;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep8 : BaseVaultCreationRoutineStep, IVaultCreationRoutineStep8
    {
        public VaultCreationRoutineStep8(VaultCreationDataModel vaultCreationDataModel)
            : base(vaultCreationDataModel)
        {
        }

        public IVaultCreationRoutineStep9 ContinueKeystoreFileInitialization()
        {
            using (vaultCreationDataModel.VaultKeystoreStream)
            {
                vaultCreationDataModel.BaseVaultKeystore.WriteKeystore(vaultCreationDataModel.VaultKeystoreStream);

                return new VaultCreationRoutineStep9(vaultCreationDataModel);
            }
        }
    }
}
