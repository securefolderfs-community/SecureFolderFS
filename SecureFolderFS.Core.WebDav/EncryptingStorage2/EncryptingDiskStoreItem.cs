using NWebDav.Server.Helpers;
using NWebDav.Server.Http;
using NWebDav.Server.Locking;
using NWebDav.Server.Props;
using NWebDav.Server.Stores;
using SecureFolderFS.Core.FileSystem.Streams;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Paths;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage2
{
    internal class EncryptingDiskStoreItem : IDiskStoreItem
    {
        private readonly IStreamsAccess _streamsAccess;
        private readonly IPathConverter _pathConverter;
        private readonly FileInfo _fileInfo;
        private readonly Security _security;

        public EncryptingDiskStoreItem(ILockingManager lockingManager, FileInfo fileInfo, bool isWritable, IStreamsAccess streamsAccess, IPathConverter pathConverter, Security security)
        {
            LockingManager = lockingManager;
            _fileInfo = fileInfo;
            _streamsAccess = streamsAccess;
            _pathConverter = pathConverter;
            _security = security;
            IsWritable = isWritable;
        }

        public static PropertyManager<EncryptingDiskStoreItem> DefaultPropertyManager { get; } = new(new DavProperty<EncryptingDiskStoreItem>[]
        {
            // RFC-2518 properties
            new DavCreationDate<EncryptingDiskStoreItem>
            {
                Getter = (context, item) => item._fileInfo.CreationTimeUtc,
                Setter = (context, item, value) =>
                {
                    item._fileInfo.CreationTimeUtc = value;
                    return HttpStatusCode.OK;
                }
            },
            new DavDisplayName<EncryptingDiskStoreItem>
            {
                Getter = (context, item) => item._fileInfo.Name
            },
            new DavGetContentLength<EncryptingDiskStoreItem>
            {
                Getter = (context, item) => Math.Max(0, item._security.ContentCrypt.CalculateCleartextSize(item._fileInfo.Length - item._security.HeaderCrypt.HeaderCiphertextSize))
            },
            new DavGetContentType<EncryptingDiskStoreItem>
            {
                Getter = (context, item) => item.DetermineContentType()
            },
            new DavGetEtag<EncryptingDiskStoreItem>
            {
                Getter = (context, item) => $"{item._fileInfo.Length}-{item._fileInfo.LastWriteTimeUtc.ToFileTime()}"
            },
            new DavGetLastModified<EncryptingDiskStoreItem>
            {
                Getter = (context, item) => item._fileInfo.LastWriteTimeUtc,
                Setter = (context, item, value) =>
                {
                    item._fileInfo.LastWriteTimeUtc = value;
                    return HttpStatusCode.OK;
                }
            },
            new DavGetResourceType<EncryptingDiskStoreItem>
            {
                Getter = (context, item) => null
            },

            // Default locking property handling via the LockingManager
            new DavLockDiscoveryDefault<EncryptingDiskStoreItem>(),
            new DavSupportedLockDefault<EncryptingDiskStoreItem>(),

            // Hopmann/Lippert collection properties
            // (although not a collection, the IsHidden property might be valuable)
            new DavExtCollectionIsHidden<EncryptingDiskStoreItem>
            {
                Getter = (context, item) => (item._fileInfo.Attributes & FileAttributes.Hidden) != 0
            },

            // Win32 extensions
            new Win32CreationTime<EncryptingDiskStoreItem>
            {
                Getter = (context, item) => item._fileInfo.CreationTimeUtc,
                Setter = (context, item, value) =>
                {
                    item._fileInfo.CreationTimeUtc = value;
                    return HttpStatusCode.OK;
                }
            },
            new Win32LastAccessTime<EncryptingDiskStoreItem>
            {
                Getter = (context, item) => item._fileInfo.LastAccessTimeUtc,
                Setter = (context, item, value) =>
                {
                    item._fileInfo.LastAccessTimeUtc = value;
                    return HttpStatusCode.OK;
                }
            },
            new Win32LastModifiedTime<EncryptingDiskStoreItem>
            {
                Getter = (context, item) => item._fileInfo.LastWriteTimeUtc,
                Setter = (context, item, value) =>
                {
                    item._fileInfo.LastWriteTimeUtc = value;
                    return HttpStatusCode.OK;
                }
            },
            new Win32FileAttributes<EncryptingDiskStoreItem>
            {
                Getter = (context, item) => item._fileInfo.Attributes,
                Setter = (context, item, value) =>
                {
                    item._fileInfo.Attributes = value;
                    return HttpStatusCode.OK;
                }
            }
        });

        public bool IsWritable { get; }
        public string Name => _pathConverter.GetCleartextFileName(_fileInfo.FullName) ?? string.Empty;
        public string UniqueKey => _fileInfo.FullName;
        public string FullPath => _pathConverter.ToCleartext(_fileInfo.FullName) ?? string.Empty;
        public Task<Stream> GetReadableStreamAsync(IHttpContext context) => Task.FromResult<Stream?>(_streamsAccess.OpenCleartextStream(_fileInfo.FullName, _fileInfo.OpenRead()));

        public async Task<HttpStatusCode> UploadFromStreamAsync(IHttpContext context, Stream inputStream)
        {
            // Check if the item is writable
            if (!IsWritable)
                return HttpStatusCode.Conflict;

            // Copy the stream
            try
            {
                // Copy the information to the destination stream
                using (var outputStream = _streamsAccess.OpenCleartextStream(_fileInfo.FullName, _fileInfo.OpenWrite()))
                {
                    await inputStream.CopyToAsync(outputStream).ConfigureAwait(false);
                }
                return HttpStatusCode.OK;
            }
            catch (IOException ioException) when (ioException.IsDiskFull())
            {
                return HttpStatusCode.InsufficientStorage;
            }
        }

        public IPropertyManager PropertyManager => DefaultPropertyManager;
        public ILockingManager LockingManager { get; }

        public async Task<StoreItemResult> CopyAsync(IStoreCollection destination, string name, bool overwrite, IHttpContext context)
        {
            try
            {
                // If the destination is also a disk-store, then we can use the FileCopy API
                // (it's probably a bit more efficient than copying in C#)
                if (destination is DiskStoreCollection diskCollection)
                {
                    // Check if the collection is writable
                    if (!diskCollection.IsWritable)
                        return new StoreItemResult(HttpStatusCode.PreconditionFailed);

                    var destinationPath = Path.Combine(diskCollection.FullPath, name);

                    // Check if the file already exists
                    var fileExists = File.Exists(destinationPath);
                    if (fileExists && !overwrite)
                        return new StoreItemResult(HttpStatusCode.PreconditionFailed);

                    // Copy the file
                    File.Copy(_fileInfo.FullName, destinationPath, true);

                    // Return the appropriate status
                    return new StoreItemResult(fileExists ? HttpStatusCode.NoContent : HttpStatusCode.Created);
                }
                else
                {
                    // Create the item in the destination collection
                    var result = await destination.CreateItemAsync(name, overwrite, context).ConfigureAwait(false);

                    // Check if the item could be created
                    if (result.Item != null)
                    {
                        using (var sourceStream = await GetReadableStreamAsync(context).ConfigureAwait(false))
                        {
                            var copyResult = await result.Item.UploadFromStreamAsync(context, sourceStream).ConfigureAwait(false);
                            if (copyResult != HttpStatusCode.OK)
                                return new StoreItemResult(copyResult, result.Item);
                        }
                    }

                    // Return result
                    return new StoreItemResult(result.Result, result.Item);
                }
            }
            catch (Exception exc)
            {
                // TODO(wd): Add logging
                //s_log.Log(LogLevel.Error, () => "Unexpected exception while copying data.", exc);
                return new StoreItemResult(HttpStatusCode.InternalServerError);
            }
        }

        public override int GetHashCode()
        {
            return _fileInfo.FullName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EncryptingDiskStoreItem storeItem))
                return false;
            return storeItem._fileInfo.FullName.Equals(_fileInfo.FullName, StringComparison.CurrentCultureIgnoreCase);
        }

        private string DetermineContentType()
        {
            return MimeTypeHelper.GetMimeType(_fileInfo.Name);
        }
    }
}
