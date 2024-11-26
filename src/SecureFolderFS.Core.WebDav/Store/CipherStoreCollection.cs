using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NWebDav.Server;
using NWebDav.Server.Props;
using NWebDav.Server.Stores;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Core.FileSystem.Helpers.Native;

namespace SecureFolderFS.Core.WebDav.Store
{
    internal sealed class CipherStoreCollection : IStoreCollection
    {
        private readonly FileSystemSpecifics _specifics;
        private readonly CipherStore _store;
        private readonly ILogger _logger;
        
        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string UniqueKey { get; }

        /// <inheritdoc/>
        public InfiniteDepthMode InfiniteDepthMode { get; } = InfiniteDepthMode.Rejected;
        
        /// <inheritdoc/>
        public IPropertyManager PropertyManager { get; }
        
        public DirectoryInfo DirectoryInfo { get; } // TODO: Not from interface
        public bool IsWritable { get; } // TODO: Not from interface

        public CipherStoreCollection(DirectoryInfo directoryInfo, IPropertyManager propertyManager, FileSystemSpecifics specifics, CipherStore store, ILogger logger)
        {
            _specifics = specifics;
            _store = store;
            _logger = logger;
            DirectoryInfo = directoryInfo;
            IsWritable = specifics.FileSystemOptions.IsReadOnly;
            UniqueKey = NativePathHelpers.GetPlaintextPath(directoryInfo.FullName, specifics) ?? string.Empty;
            Name = Path.GetFileName(UniqueKey);
            DirectoryInfo = directoryInfo;
            PropertyManager = propertyManager;
        }
        
        /// <inheritdoc/>
        public Task<Stream> GetReadableStreamAsync(CancellationToken cancellationToken)
            => Task.FromResult(Stream.Null);
        
        /// <inheritdoc/>
        public Task<DavStatusCode> UploadFromStreamAsync(Stream inputStream, CancellationToken cancellationToken)
            => Task.FromResult(DavStatusCode.Conflict);
        
        /// <inheritdoc/>
        public async Task<IStoreItem?> GetItemAsync(string name, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            cancellationToken.ThrowIfCancellationRequested();
            
            // Get the item path
            var fullPath = NativePathHelpers.GetCiphertextPath(Path.Combine(UniqueKey, name), _specifics);
            
            // Create a new item instance
            return _store.CreateFromPath<IStoreItem>(fullPath);
        }
        
        /// <inheritdoc/>
        public async IAsyncEnumerable<IStoreItem> GetItemsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            cancellationToken.ThrowIfCancellationRequested();
            
            // Add all directories
            foreach (var item in Directory.EnumerateDirectories(DirectoryInfo.FullName))
            {
                if (PathHelpers.IsCoreFile(Path.GetFileName(item)))
                    continue;
                
                var directory = _store.CreateFromPath<IStoreCollection>(item);
                if (directory is null)
                    continue;

                yield return directory;
            }

            // Add all files
            foreach (var item in Directory.EnumerateFiles(DirectoryInfo.FullName))
            {
                if (PathHelpers.IsCoreFile(Path.GetFileName(item)))
                    continue;
                
                var file = _store.CreateFromPath<IStoreItem>(item);
                if (file is null)
                    continue;

                yield return file;
            }
        }

        /// <inheritdoc/>
        public async Task<StoreItemResult> CreateItemAsync(string name, Stream stream, bool overwrite, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            // Return error
            if (!IsWritable)
                return new StoreItemResult(DavStatusCode.PreconditionFailed);

            // Determine the destination path
            var destinationPath = NativePathHelpers.GetCiphertextPath(Path.Combine(UniqueKey, name), _specifics);

            // Check if the file can be overwritten
            if (File.Exists(destinationPath) && !overwrite)
                return new StoreItemResult(DavStatusCode.PreconditionFailed);

            try
            {
                var file = File.Create(destinationPath);
                await using (file.ConfigureAwait(false))
                {
                    await stream.CopyToAsync(file, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception exc)
            {
                // Log exception
                _logger.LogError(exc, "Unable to create '{Path}' file.", destinationPath);
                return new StoreItemResult(DavStatusCode.InternalServerError);
            }

            // Return result
            var item = _store.CreateFromPath<IStoreItem>(destinationPath);
            return new StoreItemResult(DavStatusCode.Created, item);
        }
        
        /// <inheritdoc/>
        public async Task<StoreCollectionResult> CreateCollectionAsync(string name, bool overwrite, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            cancellationToken.ThrowIfCancellationRequested();
            
            // Return error
            if (!IsWritable)
                return new StoreCollectionResult(DavStatusCode.PreconditionFailed);

            // Determine the destination path
            var destinationPath = NativePathHelpers.GetCiphertextPath(Path.Combine(DirectoryInfo.FullName, name), _specifics);

            // Check if the directory can be overwritten
            DavStatusCode result;
            if (Directory.Exists(destinationPath))
            {
                // Check if overwrite is allowed
                if (!overwrite)
                    return new StoreCollectionResult(DavStatusCode.PreconditionFailed);

                // Overwrite existing
                result = DavStatusCode.NoContent;
            }
            else
            {
                // Created new directory
                result = DavStatusCode.Created;
            }

            // Attempt to create the directory
            Directory.CreateDirectory(destinationPath);

            // Return the collection
            return new StoreCollectionResult(result, _store.CreateFromPath<IStoreCollection>(destinationPath));
        }

        /// <inheritdoc/>
        public async Task<StoreItemResult> CopyAsync(IStoreCollection destinationCollection, string name, bool overwrite, CancellationToken cancellationToken)
        {
            // Just create the folder itself
            var result = await destinationCollection.CreateCollectionAsync(name, overwrite, cancellationToken).ConfigureAwait(false);
            return new StoreItemResult(result.Result, result.Collection);
        }
        
        /// <inheritdoc/>
        public bool SupportsFastMove(IStoreCollection destination, string destinationName, bool overwrite)
        {
            // We can only move disk-store collections
            return destination is CipherStoreCollection;
        }
        
        /// <inheritdoc/>
        public async Task<StoreItemResult> MoveItemAsync(string sourceName, IStoreCollection destinationCollection, string destinationName, bool overwrite, CancellationToken cancellationToken)
        {
            // Return error
            if (!IsWritable)
                return new StoreItemResult(DavStatusCode.PreconditionFailed);

            // Determine the object that is being moved
            var item = await GetItemAsync(sourceName, cancellationToken).ConfigureAwait(false);
            if (item == null)
                return new StoreItemResult(DavStatusCode.NotFound);

            try
            {
                // If the destination collection is a directory too, then we can simply move the file
                if (destinationCollection is DiskStoreCollection destinationDiskStoreCollection)
                {
                    // Return error
                    if (!destinationDiskStoreCollection.IsWritable)
                        return new StoreItemResult(DavStatusCode.PreconditionFailed);

                    // Determine source and destination paths
                    var sourcePath = NativePathHelpers.GetCiphertextPath(Path.Combine(DirectoryInfo.FullName, sourceName), _specifics);
                    var destinationPath = NativePathHelpers.GetCiphertextPath(Path.Combine(destinationDiskStoreCollection.DirectoryInfo.FullName, destinationName), _specifics);

                    // Check if the file already exists
                    DavStatusCode result;
                    if (File.Exists(destinationPath))
                    {
                        // Remove the file if it already exists (if allowed)
                        if (!overwrite)
                            return new StoreItemResult(DavStatusCode.Forbidden);

                        // The file will be overwritten
                        File.Delete(destinationPath);
                        result = DavStatusCode.NoContent;
                    }
                    else if (Directory.Exists(destinationPath))
                    {
                        // Remove the directory if it already exists (if allowed)
                        if (!overwrite)
                            return new StoreItemResult(DavStatusCode.Forbidden);

                        // The file will be overwritten
                        Directory.Delete(destinationPath, true);
                        result = DavStatusCode.NoContent;
                    }
                    else
                    {
                        // The file will be "created"
                        result = DavStatusCode.Created;
                    }

                    switch (item)
                    {
                        case DiskStoreItem _:
                            // Move the file
                            File.Move(sourcePath, destinationPath);
                            return new StoreItemResult(result, _store.CreateFromPath<IStoreItem>(destinationPath));

                        case DiskStoreCollection _:
                            // Move the directory
                            Directory.Move(sourcePath, destinationPath);
                            return new StoreItemResult(result, _store.CreateFromPath<IStoreCollection>(destinationPath));

                        default:
                            // Invalid item
                            Debug.Fail($"Invalid item {item.GetType()} inside the {nameof(DiskStoreCollection)}.");
                            return new StoreItemResult(DavStatusCode.InternalServerError);
                    }
                }
                else
                {
                    // Attempt to copy the item to the destination collection
                    var result = await item.CopyAsync(destinationCollection, destinationName, overwrite, cancellationToken).ConfigureAwait(false);
                    if (result.Result == DavStatusCode.Created || result.Result == DavStatusCode.NoContent)
                        await DeleteItemAsync(sourceName, cancellationToken).ConfigureAwait(false);

                    // Return the result
                    return result;
                }
            }
            catch (UnauthorizedAccessException)
            {
                return new StoreItemResult(DavStatusCode.Forbidden);
            }
        }
        
        /// <inheritdoc/>
        public async Task<DavStatusCode> DeleteItemAsync(string name, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            
            // Return error
            if (!IsWritable)
                return DavStatusCode.PreconditionFailed;

            // Determine the full path
            var fullPath = NativePathHelpers.GetCiphertextPath(Path.Combine(DirectoryInfo.FullName, name), _specifics);
            try
            {
                // Check if the file exists
                if (File.Exists(fullPath))
                {
                    // Delete the file
                    File.Delete(fullPath);
                    return DavStatusCode.Ok;
                }

                // Check if the directory exists
                if (Directory.Exists(fullPath))
                {
                    // Delete the directory
                    Directory.Delete(fullPath, true);
                    return DavStatusCode.Ok;
                }

                // Item not found
                return DavStatusCode.NotFound;
            }
            catch (UnauthorizedAccessException)
            {
                return DavStatusCode.Forbidden;
            }
            catch (Exception exc)
            {
                // Log exception
                _logger.LogError(exc, "Unable to delete '{Path}' directory.", fullPath);
                return DavStatusCode.InternalServerError;
            }
        }
    }
}
