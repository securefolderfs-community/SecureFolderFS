using OwlCore.Storage;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem
{
    internal sealed record SafRoot(IVFSRoot StorageRoot, string Rid);
}
