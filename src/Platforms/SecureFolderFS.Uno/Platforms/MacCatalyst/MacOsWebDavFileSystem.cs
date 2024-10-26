using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NWebDav.Server.Dispatching;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Storage.VirtualFileSystem;

#if __MACCATALYST__
using System.Diagnostics;
using OwlCore.Storage.Memory;
using SecureFolderFS.Uno.Helpers;
#endif

namespace SecureFolderFS.Uno.Platforms.Desktop
{
    /// <inheritdoc cref="IFileSystem"/>
    internal sealed partial class MacOsWebDavFileSystem : WebDavFileSystem
    {
        /// <inheritdoc/>
        protected override async Task<IVFSRoot> MountAsync(
            int port,
            string domain,
            string protocol,
            HttpListener listener,
            FileSystemOptions options,
            IRequestDispatcher requestDispatcher,
            CancellationToken cancellationToken)
        {
#if __MACCATALYST__
            var remotePath = DriveMappingHelpers.GetRemotePath(protocol, "localhost", port, options.VolumeName);
            var mountPath = await DriveMappingHelpers.GetMountPathForRemotePathAsync(remotePath);

            var webDavWrapper = new WebDavWrapper(listener, requestDispatcher, mountPath);
            webDavWrapper.StartFileSystem();

            // TODO: Remove once the port is displayed in the UI.
            Debug.WriteLine($"WebDAV server started on port {port}.");
            Debug.WriteLine($"MountableDAV\nmountPath: {mountPath}\nremotePath: {remotePath}");

            // TODO: Currently using MemoryFolder because the check in SystemFolder might sometimes fail
            return new WebDavRootFolder(webDavWrapper, new MemoryFolder(remotePath, options.VolumeName), options);
#else
            throw new PlatformNotSupportedException();
#endif
        }
    }
}
