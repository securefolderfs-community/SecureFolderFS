using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NWebDav.Server.Dispatching;
using OwlCore.Storage.Memory;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Core.FileSystem.Storage;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.Uno.Helpers;

namespace SecureFolderFS.Uno.Platforms.Windows
{
    /// <inheritdoc cref="IFileSystemInfo"/>
    public sealed class WindowsWebDavFileSystem : WebDavFileSystem
    {
        /// <inheritdoc/>
        public override Task<string> GetVolumeNameAsync(string candidateName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(candidateName);
        }
        
        /// <inheritdoc/>
        protected override async Task<IVfsRoot> MountAsync(
            FileSystemSpecifics specifics,
            HttpListener listener,
            WebDavOptions options,
            IRequestDispatcher requestDispatcher,
            CancellationToken cancellationToken)
        {
            var remotePath = DriveMappingHelpers.GetRemotePath(options.Protocol, options.Domain, options.Port, options.VolumeName);
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

            var virtualizedRoot = new MemoryFolder(mountPath, options.VolumeName);
            var plaintextRoot = new CryptoFolder(Path.DirectorySeparatorChar.ToString(), specifics.ContentFolder, specifics);
            return new WindowsWebDavVfsRoot(webDavWrapper, virtualizedRoot, plaintextRoot, specifics);
        }
    }
}
