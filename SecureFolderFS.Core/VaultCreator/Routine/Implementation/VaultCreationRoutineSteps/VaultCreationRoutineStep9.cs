using SecureFolderFS.Core.Assertions;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Enums;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep9 : BaseVaultCreationRoutineStep, IVaultCreationRoutineStep9
    {
        public VaultCreationRoutineStep9(VaultCreationDataModel vaultCreationDataModel)
            : base(vaultCreationDataModel)
        {
        }

        public IVaultCreationRoutineStep10 SetContentCipherScheme(ContentCipherScheme contentCipherScheme)
        {
            EnumAssertions.AssertCorrectContentCipherScheme(contentCipherScheme);

            vaultCreationDataModel.ContentCipherScheme = contentCipherScheme;

            return new VaultCreationRoutineStep10(vaultCreationDataModel);
        }
    }
}
