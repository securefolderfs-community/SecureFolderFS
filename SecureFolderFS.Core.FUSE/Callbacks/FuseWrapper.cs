using SecureFolderFS.Core.FileSystem.Enums;
using Tmds.Fuse;

namespace SecureFolderFS.Core.FUSE.Callbacks
{
    internal sealed class FuseWrapper
    {
        private readonly OnDeviceFuse _fuseCallbacks;
        private IFuseMount? _fuseMount;

        public FuseWrapper(OnDeviceFuse fuseCallbacks)
        {
            _fuseCallbacks = fuseCallbacks;
        }

        public void StartFileSystem(string mountPoint)
        {
            _fuseMount = Fuse.Mount(mountPoint, _fuseCallbacks, new MountOptions
            {
                Options = "default_permissions"
            });
        }

        public Task<bool> CloseFileSystemAsync(FileSystemCloseMethod closeMethod)
        {
            if (_fuseMount == null)
                throw new InvalidOperationException("The filesystem has not been started.");

            _ = closeMethod; // TODO: Implement close method
            return _fuseMount.UnmountAsync();
        }
    }
}