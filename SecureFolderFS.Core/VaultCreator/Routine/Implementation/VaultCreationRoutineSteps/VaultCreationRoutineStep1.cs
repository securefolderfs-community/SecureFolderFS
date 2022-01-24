using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Paths;
using System;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep1 : IVaultCreationRoutineStep1
    {
        private readonly VaultCreationDataModel _vaultCreationDataModel;

        public VaultCreationRoutineStep1(VaultCreationDataModel vaultCreationDataModel)
        {
            this._vaultCreationDataModel = vaultCreationDataModel;
        }

        public IVaultCreationRoutineStep2 SetVaultPath(VaultPath vaultPath)
        {
            ArgumentNullException.ThrowIfNull(vaultPath);

            _vaultCreationDataModel.VaultPath = vaultPath;

            return new VaultCreationRoutineStep2(_vaultCreationDataModel);
        }
    }
}
