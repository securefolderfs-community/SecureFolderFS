﻿using NWebDav.Server.Http;
using NWebDav.Server.Locking;
using NWebDav.Server.Stores;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Paths;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage2
{
    internal sealed class EncryptingDiskStore : DiskStore
    {
        private readonly FileSystemSpecifics _specifics;
        private readonly IPathConverter _pathConverter;

        public EncryptingDiskStore(string directory, FileSystemSpecifics specifics, IPathConverter pathConverter, bool isWritable = true, ILockingManager? lockingManager = null)
            : base(directory, isWritable, lockingManager)
        {
            _specifics = specifics;
            _pathConverter = pathConverter;
        }

        public override Task<IStoreItem> GetItemAsync(Uri uri, IHttpContext context)
        {
            // Determine the path from the uri
            var path = GetPathFromUri(uri);

            // Check if it's a directory
            if (Directory.Exists(path))
                return Task.FromResult<IStoreItem>(new EncryptingDiskStoreCollection(LockingManager, new DirectoryInfo(path), IsWritable, _specifics, _pathConverter));

            // Check if it's a file
            if (File.Exists(path))
                return Task.FromResult<IStoreItem>(new EncryptingDiskStoreItem(LockingManager, new FileInfo(path), IsWritable, _specifics, _pathConverter));

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
            return Task.FromResult<IStoreCollection>(new EncryptingDiskStoreCollection(LockingManager, new DirectoryInfo(path), IsWritable, _specifics, _pathConverter));
        }

        protected override string GetPathFromUri(Uri uri)
        {
            var path = base.GetPathFromUri(uri);
            return _pathConverter.ToCiphertext(path) ?? throw new CryptographicException("Could convert to ciphertext path.");
        }
    }
}
