using System.IO;

namespace SecureFolderFS.Core.Paths
{
    public sealed class VaultPath
    {
        public string VaultRootPath { get; } 

        public string VaultContentPath { get; }

        public VaultPath(string vaultRootPath)
        {
            VaultRootPath = vaultRootPath;
            VaultContentPath = Path.Combine(vaultRootPath, Constants.CONTENT_FOLDERNAME);
        }

        public static implicit operator string(VaultPath vaultPath) => vaultPath.VaultContentPath;
    }
}
