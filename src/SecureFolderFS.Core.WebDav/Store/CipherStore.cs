using System;
using System.IO;
using Microsoft.Extensions.Logging;
using NWebDav.Server.Props;
using NWebDav.Server.Stores;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.Native;

namespace SecureFolderFS.Core.WebDav.Store
{
    internal sealed class CipherStore : CipherStoreBase
    {
        private readonly FileSystemSpecifics _specifics;

        /// <inheritdoc/>
        public override bool IsWritable { get; }

        /// <inheritdoc/>
        public override string BaseDirectory { get; }

        public CipherStore(
            FileSystemSpecifics specifics,
            IPropertyManager itemPropertyManager,
            IPropertyManager collectionPropertyManager,
            ILogger logger)
            : base(collectionPropertyManager, itemPropertyManager, logger)
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
                return (T?)(object)new CipherStoreCollection(new(path), collectionPropertyManager, _specifics, this, logger);
            }
            //else if (typeof(T).IsAssignableFrom(typeof(IStoreFile))) // TODO: Add an IStoreFile
            else
            {
                // Check if it's a file
                if (File.Exists(path))
                    return (T?)(object)new CipherStoreItem(new(path), itemPropertyManager, _specifics, this, logger);
                
                // Check if it's a directory
                if (Directory.Exists(path))
                    return (T?)(object)new CipherStoreCollection(new(path), collectionPropertyManager, _specifics, this, logger);

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
