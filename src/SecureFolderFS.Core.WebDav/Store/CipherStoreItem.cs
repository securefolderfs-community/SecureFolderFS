using Microsoft.Extensions.Logging;
using NWebDav.Server;
using NWebDav.Server.Helpers;
using NWebDav.Server.Props;
using NWebDav.Server.Stores;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.Native;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Store
{
    internal class CipherStoreItem : IStoreItem
    {
        private readonly FileSystemSpecifics _specifics;
        private readonly CipherStore _store;
        private readonly ILogger _logger;

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string UniqueKey { get; }

        /// <inheritdoc/>
        public IPropertyManager PropertyManager { get; }

        public FileInfo FileInfo { get; } // TODO: Not from interface
        public bool IsWritable { get; } // TODO: Not from interface

        public CipherStoreItem(FileInfo fileInfo, IPropertyManager propertyManager, FileSystemSpecifics specifics, CipherStore store, ILogger logger)
        {
            _specifics = specifics;
            _store = store;
            _logger = logger;
            IsWritable = !specifics.FileSystemOptions.IsReadOnly;
            UniqueKey = NativePathHelpers.GetPlaintextPath(fileInfo.FullName, specifics) ?? string.Empty;
            Name = Path.GetFileName(UniqueKey);
            FileInfo = fileInfo;
            PropertyManager = propertyManager;
        }

        /// <inheritdoc/>
        public async Task<Stream> GetReadableStreamAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return _specifics.StreamsAccess.OpenPlaintextStream(FileInfo.FullName, FileInfo.OpenRead());
        }

        /// <inheritdoc/>
        public async Task<DavStatusCode> UploadFromStreamAsync(Stream inputStream, CancellationToken cancellationToken)
        {
            // Check if the item is writable
            if (!IsWritable)
                return DavStatusCode.Conflict;

            try
            {
                // Copy the information to the destination stream
                var outputStream = _specifics.StreamsAccess.OpenPlaintextStream(FileInfo.FullName, FileInfo.OpenWrite());
                await using (outputStream.ConfigureAwait(false))
                {
                    // Copy the stream
                    await inputStream.CopyToAsync(outputStream, cancellationToken).ConfigureAwait(false);
                }

                return DavStatusCode.Ok;
            }
            catch (IOException ioException) when (ioException.IsDiskFull())
            {
                return DavStatusCode.InsufficientStorage;
            }
        }

        /// <inheritdoc/>
        public async Task<StoreItemResult> CopyAsync(IStoreCollection destination, string name, bool overwrite, CancellationToken cancellationToken)
        {
            try
            {
                // If the destination is also a disk-store, then we can use the FileCopy API
                // (it's probably a bit more efficient than copying in C#)
                if (destination is CipherStoreCollection diskCollection)
                {
                    // Check if the collection is writable
                    if (!diskCollection.IsWritable)
                        return new StoreItemResult(DavStatusCode.PreconditionFailed);

                    var destinationPath = NativePathHelpers.GetCiphertextPath(Path.Combine(diskCollection.UniqueKey, name), _specifics);

                    // Check if the file already exists
                    var fileExists = File.Exists(destinationPath);
                    if (fileExists && !overwrite)
                        return new StoreItemResult(DavStatusCode.PreconditionFailed);

                    // Copy the file
                    File.Copy(FileInfo.FullName, destinationPath, true);

                    // Return the appropriate status
                    return new StoreItemResult(fileExists ? DavStatusCode.NoContent : DavStatusCode.Created);
                }
                else
                {
                    // Create the item in the destination collection
                    var sourceStream = await GetReadableStreamAsync(cancellationToken).ConfigureAwait(false);
                    await using (sourceStream.ConfigureAwait(false))
                    {
                        return await destination.CreateItemAsync(name, sourceStream, overwrite, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Unexpected exception while copying data.");
                return new StoreItemResult(DavStatusCode.InternalServerError);
            }
        }
    }
}
