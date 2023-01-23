using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.FUSE.AppModels;
using SecureFolderFS.Core.FUSE.Callbacks;
using Tmds.Fuse;

namespace SecureFolderFS.Core.FUSE
{
    internal sealed class FuseWrapper
    {
        private readonly BaseFuseCallbacks _fuseCallbacks;
        private IFuseMount? _fuseMount;

        public FuseWrapper(BaseFuseCallbacks fuseCallbacks)
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

            if (mountOptions.IsReadOnly)
            {
                rawOptions += ",ro";
            }
            if (mountOptions.PrintDebugInformation)
                rawOptions += ",debug";

            _fuseCallbacks.MountOptions = mountOptions;
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