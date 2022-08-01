using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Sdk.Storage.LocatableStorage;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep1 : BaseVaultCreationRoutineStep, IVaultCreationRoutineStep1
    {
        public VaultCreationRoutineStep1(VaultCreationDataModel vaultCreationDataModel)
            : base(vaultCreationDataModel)
        {
        }

        public IVaultCreationRoutineStep2 SetVaultFolder(ILocatableFolder vaultFolder)
        {
            vaultCreationDataModel.VaultPath = new(vaultFolder.Path);
            return new VaultCreationRoutineStep2(vaultCreationDataModel);
        }
    }
}
