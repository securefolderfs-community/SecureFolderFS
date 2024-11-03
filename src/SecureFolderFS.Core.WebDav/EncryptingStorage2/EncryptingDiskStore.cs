using Microsoft.Extensions.Logging;
using NWebDav.Server.Locking;
using NWebDav.Server.Stores;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.Native;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage2
{
    internal sealed class EncryptingDiskStore : DiskStoreBase
    {
        private readonly FileSystemSpecifics _specifics;

        /// <inheritdoc/>
        public override bool IsWritable { get; }

        /// <inheritdoc/>
        public override string BaseDirectory { get; }

        public EncryptingDiskStore(
            FileSystemSpecifics specifics,
            DiskStoreItemPropertyManager itemPropertyManager,
            DiskStoreCollectionPropertyManager collectionPropertyManager,
            ILoggerFactory loggerFactory)
            : base(collectionPropertyManager, itemPropertyManager, loggerFactory)
        {
            _specifics = specifics;
            IsWritable = !specifics.FileSystemOptions.IsReadOnly;
            BaseDirectory = specifics.ContentFolder.Id;
        }

        public override Task<IStoreItem> GetItemAsync(Uri uri, IHttpContext context)
        {
            // Determine the path from the uri
            var path = GetPathFromUri(uri);

            // Check if it's a directory
            if (Directory.Exists(path))
                return Task.FromResult<IStoreItem>(new EncryptingDiskStoreCollection(LockingManager, new DirectoryInfo(path), IsWritable, _specifics));

            // Check if it's a file
            if (File.Exists(path))
                return Task.FromResult<IStoreItem>(new EncryptingDiskStoreItem(LockingManager, new FileInfo(path), IsWritable, _specifics));

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
            return Task.FromResult<IStoreCollection>(new EncryptingDiskStoreCollection(LockingManager, new DirectoryInfo(path), IsWritable, _specifics));
        }

        protected override string GetPathFromUri(Uri uri)
        {
            var path = base.GetPathFromUri(uri);
            return NativePathHelpers.GetCiphertextPath(path, _specifics);
        }
    }
}
