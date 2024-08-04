using NWebDav.Server.Http;
using NWebDav.Server.Locking;
using NWebDav.Server.Stores;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage2
{
    internal sealed class EncryptingDiskStore : DiskStore
    {
        private readonly StreamsAccess _streamsAccess;
        private readonly IPathConverter _pathConverter;
        private readonly DirectoryIdCache _directoryIdCache;
        private readonly Security _security;

        public EncryptingDiskStore(string directory, StreamsAccess streamsAccess, IPathConverter pathConverter, DirectoryIdCache directoryIdCache, Security security, bool isWritable = true, ILockingManager? lockingManager = null)
            : base(directory, isWritable, lockingManager)
        {
            _streamsAccess = streamsAccess;
            _pathConverter = pathConverter;
            _directoryIdCache = directoryIdCache;
            _security = security;
        }

        public override Task<IStoreItem> GetItemAsync(Uri uri, IHttpContext context)
        {
            // Determine the path from the uri
            var path = GetPathFromUri(uri);

            // Check if it's a directory
            if (Directory.Exists(path))
                return Task.FromResult<IStoreItem>(new EncryptingDiskStoreCollection(LockingManager, new DirectoryInfo(path), IsWritable, _streamsAccess, _pathConverter, _directoryIdCache, _security));

            // Check if it's a file
            if (File.Exists(path))
                return Task.FromResult<IStoreItem>(new EncryptingDiskStoreItem(LockingManager, new FileInfo(path), IsWritable, _streamsAccess, _pathConverter, _security));

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
            return Task.FromResult<IStoreCollection>(new EncryptingDiskStoreCollection(LockingManager, new DirectoryInfo(path), IsWritable, _streamsAccess, _pathConverter, _directoryIdCache, _security));
        }

        protected override string GetPathFromUri(Uri uri)
        {
            var path = base.GetPathFromUri(uri);
            return _pathConverter.ToCiphertext(path) ?? throw new CryptographicException("Could convert to ciphertext path.");
        }
    }
}
