using Microsoft.Extensions.Logging;
using NWebDav.Server.Stores;
using System;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using NWebDav.Server.Helpers;
using NWebDav.Server.Props;

namespace SecureFolderFS.Core.WebDav.Store
{
    internal abstract class CipherStoreBase : IStore
    {
        protected readonly IPropertyManager collectionPropertyManager;
        protected readonly IPropertyManager itemPropertyManager;
        protected readonly ILogger logger;

        public abstract bool IsWritable { get; }

        public abstract string BaseDirectory { get; }

        protected CipherStoreBase(IPropertyManager collectionPropertyManager, IPropertyManager itemPropertyManager, ILogger logger)
        {
            this.collectionPropertyManager = collectionPropertyManager;
            this.itemPropertyManager = itemPropertyManager;
            this.logger = logger;
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

        public abstract T? CreateFromPath<T>(string path) where T : class, IStoreItem;
    }
}
