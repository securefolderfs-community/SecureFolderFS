﻿using System;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using System.Collections.Generic;

namespace SecureFolderFS.Core.WebDav.AppModels
{
    /// <inheritdoc cref="FileSystemOptions"/>
    public sealed class WebDavOptions : FileSystemOptions
    {
        private int _port = 4949;

        /// <summary>
        /// Gets the protocol used for the connection.
        /// </summary>
        public string Protocol { get; init; } = "http";

        /// <summary>
        /// Gets the domain address used for the connection.
        /// </summary>
        public string Domain { get; init; } = "localhost";

        /// <summary>
        /// Gets the preferred port number used for the connection.
        /// </summary>
        /// <remarks>
        /// This property does not guarantee that the specified port will be used.
        /// </remarks>
        public int Port { get => _port; init => _port = value; }

        internal void SetPortInternal(int value) => _port = value;

        /// <inheritdoc/>
        public override T? GetOption<T>(string name)
            where T : default
        {
            return (T?)(object?)(name switch
            {
                nameof(Protocol) => Protocol,
                nameof(Domain) => Domain,
                nameof(Port) => Port,
                _ => base.GetOption<T>(name)
            });
        }

        public static WebDavOptions ToOptions(IDictionary<string, object> options, IFolder contentFolder)
        {
            return new()
            {
                VolumeName = (string?)options.Get(nameof(VolumeName)) ?? throw new ArgumentNullException(nameof(VolumeName)),
                HealthStatistics = (IHealthStatistics?)options.Get(nameof(HealthStatistics)) ?? new HealthStatistics(contentFolder),
                FileSystemStatistics = (IFileSystemStatistics?)options.Get(nameof(FileSystemStatistics)) ?? new FileSystemStatistics(),
                IsReadOnly = (bool?)options.Get(nameof(IsReadOnly)) ?? false,
                IsCachingChunks = (bool?)options.Get(nameof(IsCachingChunks)) ?? true,
                IsCachingFileNames = (bool?)options.Get(nameof(IsCachingFileNames)) ?? true,

                // WebDav specific
                Protocol = (string?)options.Get(nameof(Protocol)) ?? "http",
                Domain = (string?)options.Get(nameof(Domain)) ?? "localhost",
                Port = (int?)options.Get(nameof(Port)) ?? 4949
            };
        }
    }
}