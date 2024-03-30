using NWebDav.Server;
using NWebDav.Server.Dispatching;
using NWebDav.Server.Storage;
using NWebDav.Server.Stores;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Core.WebDav.EncryptingStorage2;
using SecureFolderFS.Core.WebDav.Enums;
using SecureFolderFS.Core.WebDav.Helpers;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav
{
    /// <inheritdoc cref="IMountableFileSystem"/>
    public sealed class WebDavMountable : IMountableFileSystem, IAvailabilityChecker
    {      
        private readonly FileSystemOptions _options;
        private readonly IRequestDispatcher _requestDispatcher;

        private WebDavMountable(FileSystemOptions options, IRequestDispatcher requestDispatcher)
        {
            _options = options;
            _requestDispatcher = requestDispatcher;
        }

        /// <inheritdoc/>
        public static FileSystemAvailabilityType IsSupported()
        {
            return FileSystemAvailabilityType.Available;
        }

        /// <inheritdoc/>
        public async Task<IVFSRootFolder> MountAsync(MountOptions mountOptions, CancellationToken cancellationToken = default)
        {
            if (mountOptions is not WebDavMountOptions webDavMountOptions)
                throw new ArgumentException($"Parameter {nameof(mountOptions)} does not implement {nameof(WebDavMountOptions)}.");

            var port = webDavMountOptions.PreferredPort;
            if (port > 65536 || port <= 0)
                throw new ArgumentException($"Parameter {nameof(WebDavMountOptions.PreferredPort)} is invalid.");

            if (!PortHelpers.IsPortAvailable(port))
                port = PortHelpers.GetNextAvailablePort(port);

            var protocol = webDavMountOptions.Protocol == WebDavProtocolMode.Http ? "http" : "https";
            var prefix = $"{protocol}://{webDavMountOptions.Domain}:{port}/";
            var httpListener = new HttpListener();

            httpListener.Prefixes.Add(prefix);
            httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            var remotePath = $"\\\\localhost@{port}\\{_options.VolumeName}\\";

            string? mountPath = null;
            if (OperatingSystem.IsWindows())
            {
                mountPath = DriveMappingHelper.GetMountPathForRemotePath(remotePath);
                if (mountPath is null)
                {
                    mountPath = PathHelpers.GetFreeWindowsMountPath();
                    if (mountPath is not null)
                        _ = DriveMappingHelper.MapNetworkDriveAsync(mountPath, remotePath, cancellationToken);
                }
            }

            var webDavWrapper = new WebDavWrapper(httpListener, _requestDispatcher, mountPath);
            webDavWrapper.StartFileSystem();

            // TODO Remove once the port is displayed in the UI.
            Debug.WriteLine($"WebDAV server started on port {port}.");
            
            return new WebDavRootFolder(webDavWrapper, new SystemFolder(remotePath), _options.FileSystemStatistics);
        }

        public static IMountableFileSystem CreateMountable(FileSystemOptions options, IFolder contentFolder, Security security, DirectoryIdCache directoryIdCache, IPathConverter pathConverter, IStreamsAccess streamsAccess)
        {
            var cryptoFolder = new CryptoFolder(contentFolder, streamsAccess, pathConverter, directoryIdCache);
            var davFolder = new DavFolder(cryptoFolder);

            // TODO: Remove the following line once the new DavStorage is fully implemented.
            var encryptingDiskStore = new EncryptingDiskStore(contentFolder.Id, streamsAccess, pathConverter, directoryIdCache, security);

            var dispatcher = new WebDavDispatcher(new RootDiskStore(options.VolumeName, encryptingDiskStore), davFolder, new RequestHandlerProvider(), null);
            return new WebDavMountable(options, dispatcher);
        }
    }
}
