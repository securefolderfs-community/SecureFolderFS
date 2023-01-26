using NWebDav.Server;
using NWebDav.Server.Dispatching;
using NWebDav.Server.Stores;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Core.WebDav.EncryptingStorage;
using SecureFolderFS.Core.WebDav.Enums;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.WebDav.EncryptingStorage2;
using SecureFolderFS.Core.WebDav.Storage;

namespace SecureFolderFS.Core.WebDav
{
    /// <inheritdoc cref="IMountableFileSystem"/>
    public sealed class WebDavMountable : IMountableFileSystem, IAvailabilityChecker
    {
        private readonly IRequestDispatcher _requestDispatcher;

        private WebDavMountable(IRequestDispatcher requestDispatcher)
        {
            _requestDispatcher = requestDispatcher;
        }

        /// <inheritdoc/>
        public static FileSystemAvailabilityType IsSupported()
        {
            return FileSystemAvailabilityType.Available;
        }

        /// <inheritdoc/>
        public Task<IVirtualFileSystem> MountAsync(MountOptions mountOptions, CancellationToken cancellationToken = default)
        {
            if (mountOptions is not WebDavMountOptions webDavMountOptions)
                throw new ArgumentException($"Parameter {nameof(mountOptions)} does not implement {nameof(WebDavMountOptions)}.");

            if (!int.TryParse(webDavMountOptions.Port, out var portNumber) || portNumber > 9999 || portNumber <= 0)
                throw new ArgumentException($"Parameter {nameof(WebDavMountOptions.Port)} is invalid.");

            var protocol = webDavMountOptions.Protocol == WebDavProtocolMode.Http ? "http" : "https";
            var prefix = $"{protocol}://{webDavMountOptions.Domain}:{webDavMountOptions.Port}/";
            var httpListener = new HttpListener();

            httpListener.Prefixes.Add(prefix);
            httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            IPrincipal? serverPrincipal = null;
            var webDavWrapper = new WebDavWrapper(httpListener, serverPrincipal, _requestDispatcher);
            webDavWrapper.StartFileSystem();

            return Task.FromResult<IVirtualFileSystem>(new WebDavFileSystem(null, webDavWrapper));
        }

        public static IMountableFileSystem CreateMountable(IStorageService storageService, string volumeName, IFolder contentFolder, Security security, IDirectoryIdAccess directoryIdAccess, IPathConverter pathConverter, IStreamsAccess streamsAccess)
        {
            if (contentFolder is not ILocatableFolder locatableContentFolder)
                throw new ArgumentException($"{nameof(contentFolder)} does not implement {nameof(ILocatableFolder)}.");

            var davStorageService = new EncryptingDavStorageService(locatableContentFolder, storageService, streamsAccess, pathConverter);
            var dispatcher = new WebDavDispatcher(new EncryptingDiskStore(locatableContentFolder.Path, streamsAccess, pathConverter), davStorageService, new RequestHandlerProvider(), null);

            return new WebDavMountable(dispatcher);
        }
    }
}
