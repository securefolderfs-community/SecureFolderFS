using System;
using System.IO;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Instance.Implementation;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;

namespace SecureFolderFS.Core.VaultLoader.Routine.Implementation.VaultLoadRoutineSteps
{
    internal sealed class VaultLoadRoutineStep1 : BaseVaultLoadRoutineStep, IVaultLoadRoutineStep1
    {
        public VaultLoadRoutineStep1(VaultInstance vaultInstance, VaultLoadDataModel vaultLoadDataModel)
            : base(vaultInstance, vaultLoadDataModel)
        {
        }

        public IVaultLoadRoutineStep2 SetFolder(IFolder vaultFolder, string volumeName = null, string mountLocation = null)
        {
            if (vaultFolder is not ILocatableFolder locatableFolder)
                throw new ArgumentException($"Vault folder is not {typeof(ILocatableFolder)}.");

            vaultInstance.VaultPath = new(locatableFolder.Path);
            vaultInstance.VaultFolder = vaultFolder;
            vaultInstance.VolumeName = volumeName;
            vaultInstance.SecureFolderFSInstanceImpl.MountLocation = mountLocation;

            if (string.IsNullOrEmpty(vaultInstance.VolumeName))
                vaultInstance.VolumeName = Path.GetFileNameWithoutExtension(PathHelpers.EnsureNoTrailingPathSeparator(vaultFolder.Name));

            return new VaultLoadRoutineStep2(vaultInstance, vaultLoadDataModel);
        }
    }
}
