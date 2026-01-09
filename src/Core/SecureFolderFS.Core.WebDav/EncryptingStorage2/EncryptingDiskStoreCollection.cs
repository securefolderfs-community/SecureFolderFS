using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using NWebDav.Server;
using NWebDav.Server.Enums;
using NWebDav.Server.Locking;
using NWebDav.Server.Props;
using NWebDav.Server.Storage;
using NWebDav.Server.Stores;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Native;
using SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Native;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage2
{
    internal sealed class EncryptingDiskStoreCollection : IDavFolder
    {
        private static readonly XElement s_xDavCollection = new XElement(WebDavNamespaces.DavNs + "collection");
        private readonly DirectoryInfo _directoryInfo;
        private readonly FileSystemSpecifics _specifics;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public EnumerationDepthMode DepthMode => EnumerationDepthMode.Rejected;

        /// <inheritdoc/>
        IFolder? IWrapper<IFolder>.Inner => null;

        public EncryptingDiskStoreCollection(ILockingManager lockingManager, DirectoryInfo directoryInfo, bool isWritable, FileSystemSpecifics specifics)
        {
            _specifics = specifics;
            _directoryInfo = directoryInfo;

            Id = NativePathHelpers.GetPlaintextPath(_directoryInfo.FullName, _specifics) ?? string.Empty;
            Name = Path.GetFileName(Id);
            LockingManager = lockingManager;
            IsWritable = isWritable;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            cancellationToken.ThrowIfCancellationRequested();

            switch (type)
            {
                case StorableType.File:
                {
                    foreach (var file in _directoryInfo.GetFiles())
                    {
                        if (PathHelpers.IsCoreName(file.Name))
                            continue;

                        yield return new EncryptingDiskStoreFile(LockingManager, file, IsWritable, _specifics);
                    }

                    break;
                }

                case StorableType.Folder:
                {
                    foreach (var folder in _directoryInfo.GetDirectories())
                    {
                        if (PathHelpers.IsCoreName(folder.Name))
                            continue;

                        yield return new EncryptingDiskStoreCollection(LockingManager, folder, IsWritable, _specifics);
                    }

                    break;
                }

                case StorableType.All:
                {
                    foreach (var folder in _directoryInfo.GetDirectories())
                    {
                        if (PathHelpers.IsCoreName(folder.Name))
                            continue;

                        yield return new EncryptingDiskStoreCollection(LockingManager, folder, IsWritable, _specifics);
                    }

                    foreach (var file in _directoryInfo.GetFiles())
                    {
                        if (PathHelpers.IsCoreName(file.Name))
                            continue;

                        yield return new EncryptingDiskStoreFile(LockingManager, file, IsWritable, _specifics);
                    }

                    break;
                }
            }
        }

        /// <inheritdoc/>
        public Task<IFolderWatcher> GetFolderWatcherAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            // Folder watcher in WebDav is not supported
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            var parentDirectory = _directoryInfo.Parent;
            if (parentDirectory is null)
                return Task.FromResult<IFolder?>(null);

            return Task.FromResult<IFolder?>(new DiskStoreCollection(LockingManager, parentDirectory, IsWritable));
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> GetFirstByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            cancellationToken.ThrowIfCancellationRequested();

            // Determine the path
            var id = NativePathHelpers.GetCiphertextPath(Path.Combine(Id, name), _specifics);

            // Check if the item is a file
            if (File.Exists(id))
                return new EncryptingDiskStoreFile(LockingManager, new(id), IsWritable, _specifics);

            // Check if the item is a directory
            if (Directory.Exists(id))
                return new EncryptingDiskStoreCollection(LockingManager, new(id), IsWritable, _specifics);

            // Item not found
            throw new FileNotFoundException($"An item was not found. Name: '{name}'.");
        }

        /// <inheritdoc/>
        public async Task<IDavStorable> MoveItemAsync(IDavStorable item, IDavFolder destination, string destinationName, bool overwrite, CancellationToken cancellationToken = default)
        {
            // Return error
            if (!IsWritable)
                throw new HttpListenerException((int)HttpStatusCode.PreconditionFailed);

            try
            {
                // If the destination collection is a directory too, then we can simply move the file
                if (destination is EncryptingDiskStoreCollection destinationDiskStoreCollection)
                {
                    // Return error
                    if (!destinationDiskStoreCollection.IsWritable)
                        throw new HttpListenerException((int)HttpStatusCode.PreconditionFailed);

                    // Determine source and destination paths
                    var sourcePath = NativePathHelpers.GetCiphertextPath(item.Id, _specifics);
                    var destinationPath = NativePathHelpers.GetCiphertextPath(Path.Combine(destinationDiskStoreCollection.Id, destinationName), _specifics);

                    // Check if the file already exists
                    HttpStatusCode result;
                    if (File.Exists(destinationPath))
                    {
                        // Remove the file if it already exists (if allowed)
                        if (!overwrite)
                            throw new HttpListenerException((int)HttpStatusCode.Forbidden);

                        // The file will be overwritten
                        File.Delete(destinationPath);
                        result = HttpStatusCode.NoContent;
                    }
                    else if (Directory.Exists(destinationPath))
                    {
                        // Remove the directory if it already exists (if allowed)
                        if (!overwrite)
                            throw new HttpListenerException((int)HttpStatusCode.Forbidden);

                        // The file will be overwritten
                        Directory.Delete(destinationPath, true);
                        result = HttpStatusCode.NoContent;
                    }
                    else
                    {
                        // The file will be "created"
                        result = HttpStatusCode.Created;
                    }

                    switch (item)
                    {
                        case EncryptingDiskStoreFile _:
                            // Move the file
                            File.Move(sourcePath, destinationPath);
                            return new EncryptingDiskStoreFile(LockingManager, new FileInfo(destinationPath), IsWritable, _specifics);

                        case EncryptingDiskStoreCollection _:
                            // Move the directory
                            Directory.Move(sourcePath, destinationPath);
                            return new EncryptingDiskStoreCollection(LockingManager, new DirectoryInfo(destinationPath), IsWritable, _specifics);

                        default:
                            // Invalid item
                            Debug.Fail($"Invalid item {item.GetType()} inside the {nameof(DiskStoreCollection)}.");
                            throw new HttpListenerException((int)HttpStatusCode.InternalServerError);
                    }
                }
                else
                {
                    // Attempt to copy the item to the destination collection
                    var result = await item.CopyAsync(destination, destinationName, overwrite, cancellationToken).ConfigureAwait(false);
                    if (result.Result == HttpStatusCode.Created || result.Result == HttpStatusCode.NoContent)
                    {
                        await DeleteAsync(item, cancellationToken).ConfigureAwait(false);
                        return result.Item!;
                    }
                    else
                    {
                        throw new HttpListenerException((int)result.Result);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw new HttpListenerException((int)HttpStatusCode.Forbidden);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(IStorableChild item, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            // Return error
            if (!IsWritable)
                throw new HttpListenerException((int)HttpStatusCode.PreconditionFailed);

            // Determine the full path
            var fullPath = NativePathHelpers.GetCiphertextPath(Path.Combine(Id, item.Name), _specifics);
            try
            {
                // Check if the file exists
                if (File.Exists(fullPath))
                {
                    // Delete the file
                    NativeRecycleBinHelpers.DeleteOrRecycle(fullPath, _specifics, StorableType.File);
                    return;
                }

                // Check if the directory exists
                if (Directory.Exists(fullPath))
                {
                    // Delete the directory
                    NativeRecycleBinHelpers.DeleteOrRecycle(fullPath, _specifics, StorableType.Folder);
                    return;
                }

                // Item not found
                throw new HttpListenerException((int)HttpStatusCode.NotFound);
            }
            catch (UnauthorizedAccessException)
            {
                throw new HttpListenerException((int)HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                // Log exception
                // TODO(wd): Add logging
                //s_log.Log(LogLevel.Error, () => $"Unable to delete '{fullPath}' directory.", exc);
                throw new HttpListenerException((int)HttpStatusCode.InternalServerError);
            }
        }


        /// <inheritdoc/>
        public async Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false,
            CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            // Return error
            if (!IsWritable)
                throw new HttpListenerException((int)HttpStatusCode.Forbidden);

            // Determine the destination path
            var destinationPath = NativePathHelpers.GetCiphertextPath(Path.Combine(Id, name), _specifics);

            // Check if the directory can be overwritten
            if (Directory.Exists(destinationPath))
            {
                // Check if overwrite is allowed
                if (!overwrite)
                    throw new HttpListenerException((int)HttpStatusCode.MethodNotAllowed);
            }

            try
            {
                // Attempt to create the directory
                Directory.CreateDirectory(destinationPath);

                // Create new DirectoryID
                var directoryId = Guid.NewGuid().ToByteArray();
                var directoryIdPath = Path.Combine(destinationPath, FileSystem.Constants.Names.DIRECTORY_ID_FILENAME);

                // Initialize directory with DirectoryID
                await using var directoryIdStream = File.Open(directoryIdPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
                directoryIdStream.Write(directoryId);

                // Set DirectoryID to known IDs
                _specifics.DirectoryIdCache.CacheSet(directoryIdPath, new(directoryId));
            }
            catch (UnauthorizedAccessException)
            {
                throw new HttpListenerException((int)HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                throw new HttpListenerException((int)HttpStatusCode.InternalServerError);
            }

            // Return the collection
            return new EncryptingDiskStoreCollection(LockingManager, new DirectoryInfo(destinationPath), IsWritable, _specifics);
        }

        /// <inheritdoc/>
        public async Task<IChildFile> CreateFileAsync(string name, bool overwrite = false,
            CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            // Return error
            if (!IsWritable)
                throw new HttpListenerException((int)HttpStatusCode.Forbidden);

            // Determine the destination path
            var destinationPath = NativePathHelpers.GetCiphertextPath(Path.Combine(Id, name), _specifics);

            // Check if the file can be overwritten
            if (File.Exists(destinationPath))
            {
                if (!overwrite)
                    throw new HttpListenerException((int)HttpStatusCode.PreconditionFailed);
            }

            try
            {
                // Create a new file
                File.Create(destinationPath).Dispose();
            }
            catch (UnauthorizedAccessException)
            {
                throw new HttpListenerException((int)HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                throw new HttpListenerException((int)HttpStatusCode.InternalServerError);
            }

            // Return result
            return new EncryptingDiskStoreFile(LockingManager, new FileInfo(destinationPath), IsWritable, _specifics);
        }




        public static PropertyManager<EncryptingDiskStoreCollection> DefaultPropertyManager { get; } = new(new DavProperty<EncryptingDiskStoreCollection>[]
        {
            // RFC-2518 properties
            new DavCreationDate<EncryptingDiskStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.CreationTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.CreationTimeUtc = value;
                    return HttpStatusCode.OK;
                }
            },
            new DavDisplayName<EncryptingDiskStoreCollection>
            {
                Getter = (context, collection) =>
                {
                    return collection._directoryInfo.Name == "content"
                        // Return the name of the root directory (Name will throw, as the content folder doesn't have a DirectoryID)
                        ? context.Request.Url.Segments[1]
                        : collection.Name;
                }
            },
            new DavGetLastModified<EncryptingDiskStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.LastWriteTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.LastWriteTimeUtc = value;
                    return HttpStatusCode.OK;
                }
            },
            new DavGetResourceType<EncryptingDiskStoreCollection>
            {
                Getter = (context, collection) => new []{s_xDavCollection}
            },

            // Default locking property handling via the LockingManager
            new DavLockDiscoveryDefault<EncryptingDiskStoreCollection>(),
            new DavSupportedLockDefault<EncryptingDiskStoreCollection>(),

            // Hopmann/Lippert collection properties
            new DavExtCollectionChildCount<EncryptingDiskStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.EnumerateFiles().Count() + collection._directoryInfo.EnumerateDirectories().Count()
            },
            new DavExtCollectionIsFolder<EncryptingDiskStoreCollection>
            {
                Getter = (context, collection) => true
            },
            new DavExtCollectionIsHidden<EncryptingDiskStoreCollection>
            {
                Getter = (context, collection) => (collection._directoryInfo.Attributes & FileAttributes.Hidden) != 0
            },
            new DavExtCollectionIsStructuredDocument<EncryptingDiskStoreCollection>
            {
                Getter = (context, collection) => false
            },
            new DavExtCollectionHasSubs<EncryptingDiskStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.EnumerateDirectories().Any()
            },
            new DavExtCollectionNoSubs<EncryptingDiskStoreCollection>
            {
                Getter = (context, collection) => false
            },
            new DavExtCollectionObjectCount<EncryptingDiskStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.EnumerateFiles().Count()
            },
            new DavExtCollectionReserved<EncryptingDiskStoreCollection>
            {
                Getter = (context, collection) => !collection.IsWritable
            },
            new DavExtCollectionVisibleCount<EncryptingDiskStoreCollection>
            {
                Getter = (context, collection) =>
                    collection._directoryInfo.EnumerateDirectories().Count(di => (di.Attributes & FileAttributes.Hidden) == 0) +
                    collection._directoryInfo.EnumerateFiles().Count(fi => (fi.Attributes & FileAttributes.Hidden) == 0)
            },

            // Win32 extensions
            new Win32CreationTime<EncryptingDiskStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.CreationTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.CreationTimeUtc = value;
                    return HttpStatusCode.OK;
                }
            },
            new Win32LastAccessTime<EncryptingDiskStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.LastAccessTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.LastAccessTimeUtc = value;
                    return HttpStatusCode.OK;
                }
            },
            new Win32LastModifiedTime<EncryptingDiskStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.LastWriteTimeUtc,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.LastWriteTimeUtc = value;
                    return HttpStatusCode.OK;
                }
            },
            new Win32FileAttributes<EncryptingDiskStoreCollection>
            {
                Getter = (context, collection) => collection._directoryInfo.Attributes,
                Setter = (context, collection, value) =>
                {
                    collection._directoryInfo.Attributes = value;
                    return HttpStatusCode.OK;
                }
            }
        });

        public bool IsWritable { get; }
        public IPropertyManager PropertyManager => DefaultPropertyManager;
        public ILockingManager LockingManager { get; }

        public async Task<StoreItemResult> CopyAsync(IDavFolder destination, string name, bool overwrite, CancellationToken cancellationToken)
        {
            // Just create the folder itself
            try
            {
                var result = await destination.CreateFolderAsync(name, overwrite, cancellationToken).ConfigureAwait(false);
                return new StoreItemResult(HttpStatusCode.Created, (IDavStorable)result);
            }
            catch (HttpListenerException ex)
            {
                return new StoreItemResult((HttpStatusCode)ex.ErrorCode);
            }
        }

        public bool SupportsFastMove(IDavFolder destination, string destinationName, bool overwrite)
        {
            // We can only move disk-store collections
            return destination is EncryptingDiskStoreCollection;
        }
    }
}
