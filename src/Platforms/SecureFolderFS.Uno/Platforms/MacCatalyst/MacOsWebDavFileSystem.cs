using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Core.WebDav.AppModels;
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
            WebDavOptions options,
            IAsyncDisposable webDavInstance,
            CancellationToken cancellationToken)
        {
#if __MACCATALYST__
            var remotePath = DriveMappingHelpers.GetRemotePath(options.Protocol, "localhost", options.Port, options.VolumeName);
            var remoteUri = new Uri(remotePath);

            // Mount WebDAV volume via AppleScript
            Process.Start("/usr/bin/osascript", ["-e", $"mount volume \"{remoteUri.AbsoluteUri}\""]);
            var mountPoint = $"/Volumes/{options.VolumeName}";

            await Task.CompletedTask;
            return new WebDavRootFolder(webDavInstance, new MemoryFolder(mountPoint, options.VolumeName), options);
#else
            throw new PlatformNotSupportedException();
#endif
        }
    }
}
