using System;
using System.Net;
using System.Text.RegularExpressions;
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
            var remoteUri = new Uri(remotePath);

            // Mount WebDAV volume via AppleScript
            Process.Start("/usr/bin/osascript", $"-e mount volume \"{remoteUri.AbsoluteUri}\"");
            var mountPoint = $"/Volumes/{options.VolumeName}";
            
            // Create wrapper
            var webDavWrapper = new WebDavWrapper(listener, requestDispatcher, mountPoint);
            webDavWrapper.StartFileSystem();

            Debug.WriteLine($"Mounted {remoteUri} on {mountPoint}.");
            await Task.CompletedTask;
            return new WebDavRootFolder(webDavWrapper, new MemoryFolder(remoteUri.ToString(), options.VolumeName), options);
#else
            throw new PlatformNotSupportedException();
#endif
        }
    }
}
