using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Paths;
using System;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep1 : BaseVaultCreationRoutineStep, IVaultCreationRoutineStep1
    {
        public VaultCreationRoutineStep1(VaultCreationDataModel vaultCreationDataModel)
            : base(vaultCreationDataModel)
        {
        }

        public IVaultCreationRoutineStep2 SetVaultPath(VaultPath vaultPath)
        {
            ArgumentNullException.ThrowIfNull(vaultPath);

            vaultCreationDataModel.VaultPath = vaultPath;

            return new VaultCreationRoutineStep2(vaultCreationDataModel);
        }
    }
}
