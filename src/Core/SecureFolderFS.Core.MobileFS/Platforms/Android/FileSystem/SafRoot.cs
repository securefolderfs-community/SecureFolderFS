using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem
{
    public sealed record SafRoot(IVFSRoot StorageRoot, string RootId);
}
