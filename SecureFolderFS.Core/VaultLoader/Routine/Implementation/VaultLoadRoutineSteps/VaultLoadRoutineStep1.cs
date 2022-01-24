using System;
using System.IO;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Instance.Implementation;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.VaultLoader.Routine.Implementation.VaultLoadRoutineSteps
{
    internal sealed class VaultLoadRoutineStep1 : BaseVaultLoadRoutineStep, IVaultLoadRoutineStep1
    {
        public VaultLoadRoutineStep1(VaultInstance vaultInstance, VaultLoadDataModel vaultLoadDataModel)
            : base(vaultInstance, vaultLoadDataModel)
        {
        }

        public IVaultLoadRoutineStep2 AddVaultPath(VaultPath vaultPath, string volumeName = null, string mountLocation = null)
        {
            ArgumentNullException.ThrowIfNull(vaultPath);
            
            vaultInstance.VaultPath = vaultPath;
            vaultInstance.VolumeName = volumeName ?? Path.GetFileNameWithoutExtension(PathHelpers.EnsureNoTrailingPathSeparator(vaultPath.VaultRootPath));
            vaultInstance.SecureFolderFSInstanceImpl.MountLocation = mountLocation;

            return new VaultLoadRoutineStep2(vaultInstance, vaultLoadDataModel);
        }
    }
}
