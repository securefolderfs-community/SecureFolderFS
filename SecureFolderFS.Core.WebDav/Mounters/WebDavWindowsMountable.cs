using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Core.WebDav.Enums;
using SecureFolderFS.Core.WebDav.Http;
using SecureFolderFS.Core.WebDav.Http.Dispatching;
using SecureFolderFS.Sdk.Storage;
using System;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.WebDav.Http.Requests;
using SecureFolderFS.Core.WebDav.Http.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;

namespace SecureFolderFS.Core.WebDav.Mounters
{
    /// <inheritdoc cref="IMountableFileSystem"/>
    public sealed class WebDavWindowsMountable : IMountableFileSystem
    {
        private readonly IDavDispatcher _davDispatcher;

        private WebDavWindowsMountable(IDavDispatcher davDispatcher)
        {
            _davDispatcher = davDispatcher;
        }

        /// <inheritdoc/>
        public Task<IVirtualFileSystem> MountAsync(MountOptions mountOptions, CancellationToken cancellationToken = default)
        {
            if (mountOptions is not WebDavMountOptions webDavMountOptions)
                throw new ArgumentException($"Parameter {nameof(mountOptions)} does not implement {nameof(WebDavMountOptions)}.");

            if (!int.TryParse(webDavMountOptions.Port, out var portNumber) || (portNumber > 9999 || portNumber <= 0))
                throw new ArgumentException($"Parameter {nameof(WebDavMountOptions.Port)} is invalid.");

            var protocol = webDavMountOptions.Protocol == WebDavProtocol.Http ? "http" : "https";
            var prefix = $"{protocol}://{webDavMountOptions.Domain}:{webDavMountOptions.Port}/";
            var httpListener = new HttpListener();

            httpListener.Prefixes.Add(prefix);
            httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            IPrincipal? serverPrincipal = null;
            var webDavWrapper = new WebDavWrapper(httpListener, _davDispatcher, serverPrincipal);
            webDavWrapper.StartFileSystem();

            return Task.FromResult<IVirtualFileSystem>(new WebDavFileSystem(null, webDavWrapper));
        }

        public static IMountableFileSystem CreateMountable(IStorageService storageService, string volumeName, IFolder contentFolder, Security security, IDirectoryIdAccess directoryIdAccess, IPathConverter pathConverter, IStreamsAccess streamsAccess)
        {
            if (contentFolder is not ILocatableFolder locatableContentFolder)
                throw new ArgumentException($"{nameof(contentFolder)} does not implement {nameof(ILocatableFolder)}.");

            var requestHandlerProvider = new RequestHandlerProvider();
            var davStorageService = new DavStorageService(locatableContentFolder, storageService);
            var dispatcher = new WebDavDispatcher(davStorageService, requestHandlerProvider);

            return new WebDavWindowsMountable(dispatcher);
        }
    }
}
