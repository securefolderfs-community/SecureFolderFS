using SecureFolderFS.Core.Assertions;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Enums;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep8 : IVaultCreationRoutineStep8
    {
        private readonly VaultCreationDataModel _vaultCreationDataModel;

        public VaultCreationRoutineStep8(VaultCreationDataModel vaultCreationDataModel)
        {
            this._vaultCreationDataModel = vaultCreationDataModel;
        }

        public IVaultCreationRoutineStep9 ContinueKeystoreFileInitialization()
        {
            using (_vaultCreationDataModel.VaultKeystoreStream)
            {
                _vaultCreationDataModel.BaseVaultKeystore.WriteKeystore(_vaultCreationDataModel.VaultKeystoreStream);

                return new VaultCreationRoutineStep9(_vaultCreationDataModel);
            }
        }
    }
}
