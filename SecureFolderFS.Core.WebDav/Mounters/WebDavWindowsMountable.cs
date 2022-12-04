using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Models;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.WebDav.Enums;
using SecureFolderFS.Core.WebDav.Models;
using SecureFolderFS.Sdk.Storage;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Mounters
{
    /// <inheritdoc cref="IMountableFileSystem"/>
    public sealed class WebDavWindowsMountable : IMountableFileSystem
    {
        /// <inheritdoc/>
        public Task<IVirtualFileSystem> MountAsync(MountOptions mountOptions, CancellationToken cancellationToken = default)
        {
            if (mountOptions is not WebDavMountOptions webDavMountOptions)
                throw new ArgumentException($"Parameter {nameof(mountOptions)} does not implement {nameof(WebDavMountOptions)}.");

            var protocol = webDavMountOptions.Protocol == WebDavProtocol.Http ? "http" : "https";
            var prefix = $"{protocol}://{webDavMountOptions.Domain}:{webDavMountOptions.Port}/";
            var httpListener = new HttpListener();

            httpListener.Prefixes.Add(prefix);
            httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            //return Task.FromResult<IVirtualFileSystem>(new WebDavFileSystem());
            throw new NotImplementedException();
        }

        public static IMountableFileSystem CreateMountable(string volumeName, IFolder contentFolder, Security security, IDirectoryIdAccess directoryIdAccess, IPathConverter pathConverter, IStreamsAccess streamsAccess)
        {
            return new WebDavWindowsMountable();
        }
    }
}
