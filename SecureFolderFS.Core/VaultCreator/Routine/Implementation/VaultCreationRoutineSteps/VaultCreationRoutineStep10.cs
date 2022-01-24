using SecureFolderFS.Core.Assertions;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Enums;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep10 : IVaultCreationRoutineStep10
    {
        private readonly VaultCreationDataModel _vaultCreationDataModel;

        public VaultCreationRoutineStep10(VaultCreationDataModel vaultCreationDataModel)
        {
            this._vaultCreationDataModel = vaultCreationDataModel;
        }

        public IVaultCreationRoutineStep11 SetFileNameCipherScheme(FileNameCipherScheme fileNameCipherScheme)
        {
            EnumAssertions.AssertCorrectFileNameCipherScheme(fileNameCipherScheme);

            _vaultCreationDataModel.FileNameCipherScheme = fileNameCipherScheme;

            return new VaultCreationRoutineStep11(_vaultCreationDataModel);
        }
    }
}
