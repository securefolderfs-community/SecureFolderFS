using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NWebDav.Server.Dispatching;
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
            HttpListener listener,
            WebDavOptions options,
            IRequestDispatcher requestDispatcher,
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

            var webDavWrapper = new WebDavWrapper(listener, requestDispatcher, mountPath);
            webDavWrapper.StartFileSystem();

            // TODO: Remove once the port is displayed in the UI.
            Debug.WriteLine($"WebDAV server started on port {options.Port}.");

            // TODO: Currently using MemoryFolder because the check in SystemFolder might sometimes fail
            return new WindowsWebDavVFSRoot(webDavWrapper, new MemoryFolder(remotePath, options.VolumeName), options);
        }
    }
}