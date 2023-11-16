﻿using NWebDav.Server;
using NWebDav.Server.Dispatching;
using NWebDav.Server.Stores;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Directories;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Core.WebDav.EncryptingStorage;
using SecureFolderFS.Core.WebDav.EncryptingStorage2;
using SecureFolderFS.Core.WebDav.Enums;
using SecureFolderFS.Core.WebDav.Helpers;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System;
using System.Diagnostics;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav
{
    /// <inheritdoc cref="IMountableFileSystem"/>
    public sealed class WebDavMountable : IMountableFileSystem, IAvailabilityChecker
    {      
        private readonly IRequestDispatcher _requestDispatcher;
        private readonly string _volumeName;

        private WebDavMountable(IRequestDispatcher requestDispatcher, string volumeName)
        {
            _requestDispatcher = requestDispatcher;
            _volumeName = volumeName;
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

            var remotePath = $"\\\\localhost@{port}\\{_volumeName}\\";

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

            IPrincipal? serverPrincipal = null;
            var webDavWrapper = new WebDavWrapper(httpListener, serverPrincipal, _requestDispatcher, mountPath);
            webDavWrapper.StartFileSystem();

            // TODO Remove once the port is displayed in the UI.
            Debug.WriteLine($"WebDAV server started on port {port}.");

            return Task.FromResult<IVirtualFileSystem>(new WebDavFileSystem(new SimpleWebDavFolder(remotePath), webDavWrapper));
        }

        public static IMountableFileSystem CreateMountable(string volumeName, IFolder contentFolder, Security security, DirectoryIdCache directoryIdCache, IPathConverter pathConverter, IStreamsAccess streamsAccess, IStorageService storageService)
        {
            if (contentFolder is not ILocatableFolder locatableContentFolder)
                throw new ArgumentException($"{nameof(contentFolder)} does not implement {nameof(ILocatableFolder)}.");

            var davStorageService = new EncryptingDavStorageService(locatableContentFolder, storageService, streamsAccess, pathConverter, directoryIdCache, volumeName);
            var dispatcher = new WebDavDispatcher(new RootDiskStore(volumeName, new EncryptingDiskStore(locatableContentFolder.Path, streamsAccess, pathConverter, directoryIdCache, security)), davStorageService, new RequestHandlerProvider(), null);

            return new WebDavMountable(dispatcher, volumeName);
        }
    }
}
