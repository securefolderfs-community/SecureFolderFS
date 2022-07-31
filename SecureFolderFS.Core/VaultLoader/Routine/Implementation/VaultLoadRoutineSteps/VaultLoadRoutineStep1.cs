using System;
using System.IO;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Instance.Implementation;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.Core.VaultLoader.Routine.Implementation.VaultLoadRoutineSteps
{
    internal sealed class VaultLoadRoutineStep1 : BaseVaultLoadRoutineStep, IVaultLoadRoutineStep1
    {
        public VaultLoadRoutineStep1(VaultInstance vaultInstance, VaultLoadDataModel vaultLoadDataModel)
            : base(vaultInstance, vaultLoadDataModel)
        {
        }

        public IVaultLoadRoutineStep2 SetFolder(IModifiableFolder vaultFolder, string volumeName = null, string mountLocation = null)
        {
            vaultInstance.VaultFolder = vaultFolder;
            vaultInstance.VolumeName = volumeName;
            vaultInstance.SecureFolderFSInstanceImpl.MountLocation = mountLocation;

            if (string.IsNullOrEmpty(vaultInstance.VolumeName))
            {
                if (vaultFolder is not ILocatableFolder locatableVaultFolder)
                    throw new ArgumentException($"Could not locate {nameof(vaultFolder)}");

                vaultInstance.VolumeName = Path.GetFileNameWithoutExtension(PathHelpers.EnsureNoTrailingPathSeparator(locatableVaultFolder.Path));
            }


            return new VaultLoadRoutineStep2(vaultInstance, vaultLoadDataModel);
        }
    }
}
