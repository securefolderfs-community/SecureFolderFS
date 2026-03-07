using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NWebDav.Server.Dispatching;
using OwlCore.Storage.Memory;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.Uno.Helpers;

namespace SecureFolderFS.Uno.Platforms.Desktop
{
    /// <inheritdoc cref="IFileSystem"/>
    internal sealed partial class SkiaWebDavFileSystem : WebDavFileSystem
    {
#if __UNO_SKIA_MACOS__
        /// <inheritdoc/>
        protected override async Task<IVFSRoot> MountAsync(
            FileSystemSpecifics specifics,
            HttpListener listener,
            WebDavOptions options,
            IRequestDispatcher requestDispatcher,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var remotePath = DriveMappingHelpers.GetRemotePath(options.Protocol, options.Domain, options.Port, options.VolumeName);
            var remoteUri = new Uri(remotePath);

            // Mount WebDAV volume via AppleScript
            Process.Start("/usr/bin/osascript", ["-e", $"mount volume \"{remoteUri.AbsoluteUri}\""]);
            var mountPoint = $"/Volumes/{options.VolumeName}";

            // Create wrapper
            var webDavWrapper = new WebDavWrapper(listener, requestDispatcher, mountPoint);
            webDavWrapper.StartFileSystem();

            Debug.WriteLine($"Mounted {remoteUri} on {mountPoint}.");
            return new WebDavVFSFolder(webDavWrapper, new MemoryFolder(mountPoint, options.VolumeName), specifics);
        }
#endif
    }
}
