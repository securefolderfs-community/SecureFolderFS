using SecureFolderFS.Core.Assertions;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Enums;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep9 : IVaultCreationRoutineStep9
    {
        private readonly VaultCreationDataModel _vaultCreationDataModel;

        public VaultCreationRoutineStep9(VaultCreationDataModel vaultCreationDataModel)
        {
            this._vaultCreationDataModel = vaultCreationDataModel;
        }
        public IVaultCreationRoutineStep10 SetContentCipherScheme(ContentCipherScheme contentCipherScheme)
        {
            EnumAssertions.AssertCorrectContentCipherScheme(contentCipherScheme);

            _vaultCreationDataModel.ContentCipherScheme = contentCipherScheme;

            return new VaultCreationRoutineStep10(_vaultCreationDataModel);
        }
    }
}
