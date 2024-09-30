using NWebDav.Server;
using NWebDav.Server.Dispatching;
using NWebDav.Server.Storage;
using NWebDav.Server.Stores;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Core.WebDav.EncryptingStorage2;
using SecureFolderFS.Core.WebDav.Enums;
using SecureFolderFS.Core.WebDav.Helpers;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage.Memory;

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
        public async Task<IVFSRoot> MountAsync(MountOptions mountOptions, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            if (mountOptions is not WebDavMountOptions webDavMountOptions)
                throw new ArgumentException(
                    $"Parameter {nameof(mountOptions)} does not implement {nameof(WebDavMountOptions)}.");

            var port = webDavMountOptions.PreferredPort;
            if (port > 65536 || port <= 0)
                throw new ArgumentException($"Parameter {nameof(WebDavMountOptions.PreferredPort)} is invalid.");

            if (!PortHelpers.IsPortAvailable(port))
                port = PortHelpers.GetNextAvailablePort(port);

            var protocol = webDavMountOptions.Protocol == WebDavProtocolMode.Http ? "http" : "https";
            var remotePath = DriveMappingHelpers.GetRemotePath(protocol, "localhost", port, _options.VolumeName);
            var prefix = $"{protocol}://{webDavMountOptions.Domain}:{port}/";

            var httpListener = new HttpListener();
            httpListener.Prefixes.Add(prefix);
            httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            var mountPath = await DriveMappingHelpers.GetMountPathForRemotePathAsync(remotePath);
            if (mountPath is null)
            {
                mountPath = PathHelpers.GetFreeMountPath(_options.VolumeName);
                if (mountPath is null)
                    throw new IOException("No free mount points found.");
                
                await DriveMappingHelpers.MapNetworkDriveAsync(mountPath, remotePath, cancellationToken);    
            }

            var webDavWrapper = new WebDavWrapper(httpListener, _requestDispatcher, mountPath);
            webDavWrapper.StartFileSystem();

            // TODO: Remove once the port is displayed in the UI.
            Debug.WriteLine($"WebDAV server started on port {port}.");
            Debug.WriteLine($"MountableDAV\nmountPath: {mountPath}\nremotePath: {remotePath}");

            // TODO: Currently using MemoryFolder because the check in SystemFolder might sometimes fail
            return new WebDavRootFolder(webDavWrapper, new MemoryFolder(remotePath, _options.VolumeName), _options);
        }

        public static IMountableFileSystem CreateMountable(FileSystemSpecifics specifics, IPathConverter pathConverter)
        {
            // TODO: Implement FileSystemSpecifics
            var cryptoFolder = (IFolder)null!; // new CryptoFolder(contentFolder, streamsAccess, pathConverter, directoryIdCache);
            var davFolder = new DavFolder(cryptoFolder);

            // TODO: Remove the following line once the new DavStorage is fully implemented.
            var encryptingDiskStore = new EncryptingDiskStore(specifics.ContentFolder.Id, specifics, pathConverter);
            var dispatcher = new WebDavDispatcher(new RootDiskStore(specifics.FileSystemOptions.VolumeName, encryptingDiskStore), davFolder, new RequestHandlerProvider(), null);
            return new WebDavMountable(specifics.FileSystemOptions, dispatcher);
        }
    }
}
