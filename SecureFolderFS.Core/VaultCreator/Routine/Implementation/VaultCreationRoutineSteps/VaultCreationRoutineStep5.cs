using System.IO;
using SecureFolderFS.Core.DataModels;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep5 : IVaultCreationRoutineStep5
    {
        private readonly VaultCreationDataModel _vaultCreationDataModel;

        public VaultCreationRoutineStep5(VaultCreationDataModel vaultCreationDataModel)
        {
            this._vaultCreationDataModel = vaultCreationDataModel;
        }

        public IVaultCreationRoutineStep6 CreateContentFolder()
        {
            _vaultCreationDataModel.DirectoryOperations.CreateDirectory(_vaultCreationDataModel.VaultPath.VaultContentPath);

            return new VaultCreationRoutineStep6(_vaultCreationDataModel);
        }
    }
}
