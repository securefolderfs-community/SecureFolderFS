using System.IO;
using SecureFolderFS.Core.DataModels;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep5 : BaseVaultCreationRoutineStep, IVaultCreationRoutineStep5
    {
        public VaultCreationRoutineStep5(VaultCreationDataModel vaultCreationDataModel)
            : base(vaultCreationDataModel)
        {
        }

        public IVaultCreationRoutineStep6 CreateContentFolder()
        {
            vaultCreationDataModel.DirectoryOperations.CreateDirectory(vaultCreationDataModel.VaultPath.VaultContentPath);

            return new VaultCreationRoutineStep6(vaultCreationDataModel);
        }
    }
}
