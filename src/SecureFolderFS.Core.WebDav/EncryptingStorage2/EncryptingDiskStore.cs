using Microsoft.Extensions.Logging;
using NWebDav.Server.Stores;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.Native;
using SecureFolderFS.Core.WebDav.Base;
using System;
using System.IO;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage2
{
    internal sealed class EncryptingDiskStore : DiskStoreBase2
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

        public override T? CreateFromPath<T>(string path)
            where T : class
        {
            if (typeof(T).IsAssignableFrom(typeof(IStoreCollection)))
            {
                return (T?)(object)new EncryptingDiskStoreCollection(new DirectoryInfo(path));
            }
            //else if (typeof(T).IsAssignableFrom(typeof(IStoreFile))) // TODO: Add a StoreFile
            else
            {
                // Check if it's a directory
                if (Directory.Exists(path))
                    return (T?)(object)new EncryptingDiskStoreCollection(new DirectoryInfo(path));

                // Check if it's a file
                if (File.Exists(path))
                    return (T?)(object)new EncryptingDiskStoreItem(new FileInfo(path));

                // The item doesn't exist
                return null;
            }
        }

        /// <inheritdoc/>
        protected override string GetPathFromUri(Uri uri)
        {
            var path = base.GetPathFromUri(uri);
            return NativePathHelpers.GetCiphertextPath(path, _specifics);
        }
    }
}
