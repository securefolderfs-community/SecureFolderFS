using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.FUSE.AppModels;
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

        public void StartFileSystem(FuseMountOptions mountOptions)
        {
            var rawOptions = "default_permissions";
            if (mountOptions.AllowOtherUserAccess)
                rawOptions += ",allow_other";
            else if (mountOptions.AllowRootUserAccess)
                rawOptions += ",allow_root";

            _fuseMount = Fuse.Mount(mountOptions.MountPoint, _fuseCallbacks, new MountOptions
            {
                Options = rawOptions
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