using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Storage.Extensions
{
    public static class VirtualFileSystemExtensions
    {
        public static bool IsRecycleBinEnabled(this FileSystemOptions options)
        {
            return options.RecycleBinSize != 0;
        }
    }
}
