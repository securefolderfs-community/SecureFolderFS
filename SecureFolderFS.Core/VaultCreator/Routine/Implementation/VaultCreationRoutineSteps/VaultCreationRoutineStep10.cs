using SecureFolderFS.Core.Assertions;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Enums;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep10 : BaseVaultCreationRoutineStep, IVaultCreationRoutineStep10
    {
        public VaultCreationRoutineStep10(VaultCreationDataModel vaultCreationDataModel)
            : base(vaultCreationDataModel)
        {
        }

        public IVaultCreationRoutineStep11 SetFileNameCipherScheme(FileNameCipherScheme fileNameCipherScheme)
        {
            EnumAssertions.AssertCorrectFileNameCipherScheme(fileNameCipherScheme);

            vaultCreationDataModel.FileNameCipherScheme = fileNameCipherScheme;

            return new VaultCreationRoutineStep11(vaultCreationDataModel);
        }
    }
}
