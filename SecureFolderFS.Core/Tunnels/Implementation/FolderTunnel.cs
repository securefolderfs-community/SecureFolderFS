using System;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Storage;

namespace SecureFolderFS.Core.Tunnels.Implementation
{
    internal sealed class FolderTunnel : IFolderTunnel
    {
        public bool TransferFolderOutsideOfVault(IVaultFolder vaultFolder, string destinationPath)
        {
            throw new NotImplementedException();
        }

        public IVaultFolder TransferFolderToVault(string sourcePath, ICleartextPath destinationCleartextPath)
        {
            throw new NotImplementedException();
        }
    }
}
