using SecureFolderFS.Core.DataModels;
using System;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal abstract class BaseVaultCreationRoutineStep : IDisposable
    {
        protected readonly VaultCreationDataModel vaultCreationDataModel;

        protected BaseVaultCreationRoutineStep(VaultCreationDataModel vaultCreationDataModel)
        {
            vaultCreationDataModel = vaultCreationDataModel;
        }

        public void Dispose()
        {
            vaultCreationDataModel.Cleanup();
        }
    }
}
