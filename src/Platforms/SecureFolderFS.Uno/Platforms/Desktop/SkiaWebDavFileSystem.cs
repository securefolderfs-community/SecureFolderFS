using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NWebDav.Server.Dispatching;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Uno.Platforms.Desktop
{
    /// <inheritdoc cref="IFileSystem"/>
    internal sealed partial class SkiaWebDavFileSystem : WebDavFileSystem
    {
#if WINDOWS
        /// <inheritdoc/>
        protected override Task<IVFSRoot> MountAsync(
            int port,
            string domain,
            string protocol,
            HttpListener listener,
            FileSystemOptions options,
            IRequestDispatcher requestDispatcher,
            CancellationToken cancellationToken)
        {
            throw new PlatformNotSupportedException();
        }
#endif
    }
}
