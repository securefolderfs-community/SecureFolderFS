using CommandLine;
using SecureFolderFS.Cli.Attributes;
using SecureFolderFS.Core;

namespace SecureFolderFS.Cli.Options
{
    [Verb("unlock-vault", HelpText = "Unlock a vault.")]
    public sealed class UnlockVaultOptions
    {
        [Option('v', "vault-folder", Required = true, HelpText = "Path to a folder where the vault should be created. If the specified folder does not exist, it will be created.")]
        public string VaultFolder { get; private set; }
        
        [ValidOptions(Constants.FileSystemId.WEBDAV_ID, Constants.FileSystemId.DOKAN_ID, Constants.FileSystemId.FUSE_ID)]
        [Option('f', "filesystem", Required = true, Default = Constants.FileSystemId.WEBDAV_ID, HelpText = $"TODO. Valid values (case insensitive): {Constants.FileSystemId.WEBDAV_ID}, {Constants.FileSystemId.DOKAN_ID} (Windows only), {Constants.FileSystemId.FUSE_ID} (Linux only).")]
        public string Filesystem { get; private set; }
        
        [Option("webdav-port", Default = 4949, HelpText = "TODO")]
        public int WebdavPort { get; private set; }
        
        [Option("fuse-mount-point", HelpText = "[FUSE] Path to the folder where the filesystem should be mounted. No existing filesystem must be mounted in the folder. If not specified, the filesystem will be mounted at ~/.local/share/SecureFolderFS/mount.")]
        public string? FuseMountPoint { get; private set; }
        
        [Option("fuse-allow-root", HelpText = "[FUSE] Whether to allow root to access the filesystem. Requires user_allow_other to be uncommented in /etc/fuse.conf. Mutually exclusive with fuse-allow-other.", SetName = "fuseallow")]
        public bool FuseAllowRoot { get; private set; }
        
        [Option("fuse-allow-other", HelpText = "[FUSE] Whether to allow other users, including root, to access the filesystem. Requires user_allow_other to be uncommented in /etc/fuse.conf. Mutually exclusive with fuse-allow-root.", SetName = "fuseallow")]
        public bool FuseAllowOther { get; private set; }
        
        [Option("fuse-debug", HelpText = "[FUSE] Whether to enable printing debug information to the console. This option may negatively affect the filesystem performance.")]
        public bool FuseDebug { get; private set; }
    }
}