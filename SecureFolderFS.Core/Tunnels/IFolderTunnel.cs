using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Storage;

namespace SecureFolderFS.Core.Tunnels
{
    /// <summary>
    /// Provides module for transferring folders in and out of vault securely and reliably.
    /// <br/>
    /// <br/>
    /// This API is exposed.
    /// </summary>
    public interface IFolderTunnel
    {
        IVaultFolder TransferFolderToVault(string sourcePath, ICleartextPath destinationCleartextPath);

        bool TransferFolderOutsideOfVault(IVaultFolder vaultFolder, string destinationPath);
    }
}
