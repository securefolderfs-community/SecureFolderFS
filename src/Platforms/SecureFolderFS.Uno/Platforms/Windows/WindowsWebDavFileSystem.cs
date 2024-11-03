using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage.Memory;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.Uno.Helpers;

namespace SecureFolderFS.Uno.Platforms.Windows
{
    /// <inheritdoc cref="IFileSystem"/>
    public sealed class WindowsWebDavFileSystem : WebDavFileSystem
    {
        /// <inheritdoc/>
        protected override async Task<IVFSRoot> MountAsync(
            WebDavOptions options,
            IAsyncDisposable webDavInstance,
            CancellationToken cancellationToken)
        {
            var remotePath = DriveMappingHelpers.GetRemotePath(options.Protocol, "localhost", options.Port, options.VolumeName);
            var mountPath = await DriveMappingHelpers.GetMountPathForRemotePathAsync(remotePath);
            if (mountPath is null)
            {
                mountPath = PathHelpers.GetFreeMountPath(options.VolumeName);
                if (mountPath is null)
                    throw new IOException("No free mount points found.");

                await DriveMappingHelpers.MapNetworkDriveAsync(mountPath, remotePath, cancellationToken);
            }

            return new WindowsWebDavVFSRoot(webDavInstance, new MemoryFolder(remotePath, options.VolumeName), options);
        }
    }
}
