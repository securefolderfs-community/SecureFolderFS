using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.FileSystem.Operations.Implementation;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep2 : IVaultCreationRoutineStep2
    {
        private readonly VaultCreationDataModel _vaultCreationDataModel;

        public VaultCreationRoutineStep2(VaultCreationDataModel vaultCreationDataModel)
        {
            this._vaultCreationDataModel = vaultCreationDataModel;
        }

        public IVaultCreationRoutineStep3 AddFileOperations(IFileOperations fileOperations = null, IDirectoryOperations directoryOperations = null)
        {
            _vaultCreationDataModel.FileOperations = fileOperations ?? new BuiltinFileOperations();
            _vaultCreationDataModel.DirectoryOperations = directoryOperations ?? new BuiltinDirectoryOperations();

            return new VaultCreationRoutineStep3(_vaultCreationDataModel);
        }
    }
}
