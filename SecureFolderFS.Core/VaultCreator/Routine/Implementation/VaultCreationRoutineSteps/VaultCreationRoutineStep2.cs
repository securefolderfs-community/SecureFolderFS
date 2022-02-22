using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.FileSystem.Operations.Implementation;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep2 : BaseVaultCreationRoutineStep, IVaultCreationRoutineStep2
    {
        public VaultCreationRoutineStep2(VaultCreationDataModel vaultCreationDataModel)
            : base(vaultCreationDataModel)
        {
        }

        public IVaultCreationRoutineStep3 AddFileOperations(IFileOperations fileOperations = null, IDirectoryOperations directoryOperations = null)
        {
            vaultCreationDataModel.FileOperations = fileOperations ?? new BuiltinFileOperations();
            vaultCreationDataModel.DirectoryOperations = directoryOperations ?? new BuiltinDirectoryOperations();

            return new VaultCreationRoutineStep3(vaultCreationDataModel);
        }
    }
}
