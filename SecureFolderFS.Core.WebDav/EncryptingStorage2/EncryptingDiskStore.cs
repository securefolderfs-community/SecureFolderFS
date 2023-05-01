﻿using NWebDav.Server.Http;
using NWebDav.Server.Locking;
using NWebDav.Server.Stores;
using SecureFolderFS.Core.FileSystem.Streams;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Paths;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage2
{
    internal sealed class EncryptingDiskStore : DiskStore
    {
        private readonly IStreamsAccess _streamsAccess;
        private readonly IPathConverter _pathConverter;
        private readonly IDirectoryIdAccess _directoryIdAccess;

        public EncryptingDiskStore(string directory, IStreamsAccess streamsAccess, IPathConverter pathConverter, IDirectoryIdAccess directoryIdAccess, bool isWritable = true, ILockingManager? lockingManager = null)
            : base(directory, isWritable, lockingManager)
        {
            _streamsAccess = streamsAccess;
            _pathConverter = pathConverter;
            _directoryIdAccess = directoryIdAccess;
        }

        public override Task<IStoreItem> GetItemAsync(Uri uri, IHttpContext context)
        {
            // Determine the path from the uri
            var path = GetPathFromUri(uri);

            // Check if it's a directory
            if (Directory.Exists(path))
                return Task.FromResult<IStoreItem>(new EncryptingDiskStoreCollection(LockingManager, new DirectoryInfo(path), IsWritable, _streamsAccess, _pathConverter, _directoryIdAccess));

            // Check if it's a file
            if (File.Exists(path))
                return Task.FromResult<IStoreItem>(new EncryptingDiskStoreItem(LockingManager, new FileInfo(path), IsWritable, _streamsAccess, _pathConverter));

            // The item doesn't exist
            return Task.FromResult<IStoreItem>(null);
        }

        public override Task<IStoreCollection> GetCollectionAsync(Uri uri, IHttpContext context)
        {
            // Determine the path from the uri
            var path = GetPathFromUri(uri);
            if (!Directory.Exists(path))
                return Task.FromResult<IStoreCollection>(null);

            // Return the item
            return Task.FromResult<IStoreCollection>(new EncryptingDiskStoreCollection(LockingManager, new DirectoryInfo(path), IsWritable, _streamsAccess, _pathConverter, _directoryIdAccess));
        }

        protected override string GetPathFromUri(Uri uri)
        {
            var path = base.GetPathFromUri(uri);
            return _pathConverter.ToCiphertext(path) ?? throw new CryptographicException("Could convert to ciphertext path.");
        }
    }
}
