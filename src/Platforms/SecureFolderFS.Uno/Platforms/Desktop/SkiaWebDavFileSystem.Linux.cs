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
    internal sealed partial class SkiaWebDavFileSystem
    {
#if HAS_UNO_SKIA && !__MACCATALYST__
        /// <inheritdoc/>
        protected override async Task<IVFSRoot> MountAsync(
            WebDavOptions options,
            IAsyncDisposable webDavInstance,
            CancellationToken cancellationToken)
        {
            var remotePath = DriveMappingHelpers.GetRemotePath(options.Protocol, options.Domain, options.Port, options.VolumeName);
            var mountPath = await DriveMappingHelpers.GetMountPathForRemotePathAsync(remotePath);

            // TODO: Currently using MemoryFolder because the check in SystemFolder might sometimes fail
            return new WebDavRootFolder(webDavInstance, new MemoryFolder(remotePath, options.VolumeName), options);
        }
#endif
    }
}
