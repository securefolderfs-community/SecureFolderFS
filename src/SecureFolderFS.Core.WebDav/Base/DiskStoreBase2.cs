using Microsoft.Extensions.Logging;
using NWebDav.Server.Stores;
using System;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Base
{
    internal abstract class DiskStoreBase2 : IStore
    {
        private readonly DiskStoreCollectionPropertyManager _diskStoreCollectionPropertyManager;
        private readonly DiskStoreItemPropertyManager _diskStoreItemPropertyManager;
        private readonly ILoggerFactory _loggerFactory;

        public abstract bool IsWritable { get; }

        public abstract string BaseDirectory { get; }

        protected DiskStoreBase2(DiskStoreCollectionPropertyManager diskStoreCollectionPropertyManager, DiskStoreItemPropertyManager diskStoreItemPropertyManager, ILoggerFactory loggerFactory)
        {
            _diskStoreCollectionPropertyManager = diskStoreCollectionPropertyManager;
            _diskStoreItemPropertyManager = diskStoreItemPropertyManager;
            _loggerFactory = loggerFactory;
        }

        public virtual async Task<IStoreItem?> GetItemAsync(Uri uri, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            cancellationToken.ThrowIfCancellationRequested();

            // Get path and item
            var path = GetPathFromUri(uri);
            var item = CreateFromPath<IStoreItem>(path);

            return item;
        }

        public virtual async Task<IStoreCollection?> GetCollectionAsync(Uri uri, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            cancellationToken.ThrowIfCancellationRequested();

            // Determine the path from the uri
            var path = GetPathFromUri(uri);
            if (!Directory.Exists(path))
                return null;

            // Return the item
            return CreateFromPath<IStoreCollection>(path);
        }

        protected virtual string GetPathFromUri(Uri uri)
        {
            // Determine the path
            var requestedPath = UriHelper.GetDecodedPath(uri)[1..].Replace('/', Path.DirectorySeparatorChar);

            // Determine the full path
            var fullPath = Path.GetFullPath(Path.Combine(BaseDirectory, requestedPath));

            // Make sure we're still inside the specified directory
            if (fullPath != BaseDirectory && !fullPath.StartsWith(BaseDirectory + Path.DirectorySeparatorChar))
                throw new SecurityException($"Uri '{uri}' is outside the '{BaseDirectory}' directory.");

            // Return the combined path
            return fullPath;
        }

        public virtual T? CreateFromPath<T>(string path)
            where T : class, IStoreItem
        {
            if (typeof(T).IsAssignableFrom(typeof(IStoreCollection)))
            {
                return (T?)(object)CreateCollection(new DirectoryInfo(path));
            }
            //else if (typeof(T).IsAssignableFrom(typeof(IStoreFile))) // TODO: Add a StoreFile
            else
            {
                // Check if it's a directory
                if (Directory.Exists(path))
                    return (T?)(object)CreateCollection(new DirectoryInfo(path));

                // Check if it's a file
                if (File.Exists(path))
                    return (T?)(object)CreateItem(new FileInfo(path));

                // The item doesn't exist
                return null;
            }

            internal DiskStoreCollection CreateCollection(DirectoryInfo directoryInfo) =>
            new(this, _diskStoreCollectionPropertyManager, directoryInfo, _loggerFactory.CreateLogger<DiskStoreCollection>());

        internal DiskStoreItem CreateItem(FileInfo file) =>
            new(this, _diskStoreItemPropertyManager, file, _loggerFactory.CreateLogger<DiskStoreItem>());
    }
}
