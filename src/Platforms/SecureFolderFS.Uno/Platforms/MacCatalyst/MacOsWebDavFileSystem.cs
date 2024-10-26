using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using SecureFolderFS.Storage.VirtualFileSystem;
using System.Threading.Tasks;
using OwlCore.Storage.Memory;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Uno.Helpers;
using NWebDav.Server.Dispatching;

namespace SecureFolderFS.Uno.Platforms.Desktop
{
    /// <inheritdoc cref="IFileSystem"/>
    internal sealed partial class MacOsWebDavFileSystem
    {
#if HAS_UNO_SKIA && __MACCATALYST__
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
            var remotePath = DriveMappingHelpers.GetRemotePath(protocol, "localhost", port, options.VolumeName);
            var mountPath = await DriveMappingHelpers.GetMountPathForRemotePathAsync(remotePath);

            var webDavWrapper = new WebDavWrapper(listener, requestDispatcher, mountPath);
            webDavWrapper.StartFileSystem();

            // TODO: Remove once the port is displayed in the UI.
            Debug.WriteLine($"WebDAV server started on port {port}.");
            Debug.WriteLine($"MountableDAV\nmountPath: {mountPath}\nremotePath: {remotePath}");

            // TODO: Currently using MemoryFolder because the check in SystemFolder might sometimes fail
            return new WebDavRootFolder(webDavWrapper, new MemoryFolder(remotePath, options.VolumeName), options);
        }
    }
#endif
}
