using FuseSharp;
using SecureFolderFS.Core.MacFuse.AppModels;
using SecureFolderFS.Core.MacFuse.Callbacks;

namespace SecureFolderFS.Core.MacFuse
{
    internal sealed class MacFuseWrapper
    {
        private readonly BaseMacFuseCallbacks _fuseCallbacks;
        private IFuseMount? _fuseMount;

        public MacFuseWrapper(BaseMacFuseCallbacks fuseCallbacks)
        {
            _fuseCallbacks = fuseCallbacks;
        }

        public void StartFileSystem(string mountPoint, MacFuseOptions mountOptions)
        {
            var rawOptions = $"default_permissions,fsname={nameof(SecureFolderFS)},volname={SanitizeOptionValue(mountOptions.VolumeName)}";

            // Extended attributes are forwarded to the ciphertext folder,
            // so AppleDouble (._) companion files are not needed
            rawOptions += ",noappledouble";

            if (mountOptions.AllowOtherUserAccess)
                rawOptions += ",allow_other";
            else if (mountOptions.AllowRootUserAccess)
                rawOptions += ",allow_root";

            if (mountOptions.IsReadOnly)
                rawOptions += ",ro";

            if (mountOptions.PrintDebugInformation)
                rawOptions += ",debug";

            _fuseCallbacks.FuseOptions = mountOptions;
            _fuseMount = Fuse.Mount(mountPoint, _fuseCallbacks, new MountOptions()
            {
                Options = rawOptions
            });
        }

        public Task<bool> CloseFileSystemAsync()
        {
            if (_fuseMount is null)
                throw new InvalidOperationException("The filesystem has not been started.");

            return _fuseMount.UnmountAsync(force: true);
        }

        private static string SanitizeOptionValue(string value)
        {
            // Commas separate mount options and cannot appear inside a value
            return value.Replace(',', '_');
        }
    }
}
