﻿using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NWebDav.Server.Helpers;
using NWebDav.Server.Locking;
using NWebDav.Server.Props;
using NWebDav.Server.Storage;
using NWebDav.Server.Stores;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Native;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage2
{
    internal class EncryptingDiskStoreFile : IDavFile
    {
        private readonly FileSystemSpecifics _specifics;
        private readonly FileInfo _fileInfo;

        /// <inheritdoc/>
        public string Name => Path.GetFileName(Id);

        /// <inheritdoc/>
        public string Id => NativePathHelpers.GetPlaintextPath(_fileInfo.FullName, _specifics) ?? string.Empty;

        /// <inheritdoc/>
        IFile? IWrapper<IFile>.Inner => null;

        public EncryptingDiskStoreFile(ILockingManager lockingManager, FileInfo fileInfo, bool isWritable, FileSystemSpecifics specifics)
        {
            LockingManager = lockingManager;
            IsWritable = isWritable;
            _fileInfo = fileInfo;
            _specifics = specifics;
        }

        /// <inheritdoc/>
        public Task<Stream> OpenStreamAsync(FileAccess accessMode, CancellationToken cancellationToken = default)
        {
            var baseStream = accessMode switch
            {
                FileAccess.Read => _fileInfo.OpenRead(),
                FileAccess.Write => _fileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write),
                FileAccess.ReadWrite => _fileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite),
                _ => throw new ArgumentOutOfRangeException(nameof(accessMode), accessMode, null)
            };

            var plaintextStream = _specifics.StreamsAccess.OpenPlaintextStream(_fileInfo.FullName, baseStream);
            return Task.FromResult(plaintextStream);
        }

        /// <inheritdoc/>
        public Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            var parentDirectory = _fileInfo.Directory;
            if (parentDirectory is null)
                return Task.FromResult<IFolder?>(null);

            return Task.FromResult<IFolder?>(new EncryptingDiskStoreCollection(LockingManager, parentDirectory, IsWritable, _specifics));
        }

        public static PropertyManager<EncryptingDiskStoreFile> DefaultPropertyManager { get; } = new(new DavProperty<EncryptingDiskStoreFile>[]
        {
            // RFC-2518 properties
            new DavCreationDate<EncryptingDiskStoreFile>
            {
                Getter = (context, item) => item._fileInfo.CreationTimeUtc,
                Setter = (context, item, value) =>
                {
                    item._fileInfo.CreationTimeUtc = value;
                    return HttpStatusCode.OK;
                }
            },
            new DavDisplayName<EncryptingDiskStoreFile>
            {
                Getter = (context, item) => item.Name
            },
            new DavGetContentLength<EncryptingDiskStoreFile>
            {
                Getter = (context, item) => Math.Max(0, item._specifics.Security.ContentCrypt.CalculatePlaintextSize(item._fileInfo.Length - item._specifics.Security.HeaderCrypt.HeaderCiphertextSize))
            },
            new DavGetContentType<EncryptingDiskStoreFile>
            {
                Getter = (context, item) => item.DetermineContentType()
            },
            new DavGetEtag<EncryptingDiskStoreFile>
            {
                Getter = (context, item) => $"{item._fileInfo.Length}-{item._fileInfo.LastWriteTimeUtc.ToFileTime()}"
            },
            new DavGetLastModified<EncryptingDiskStoreFile>
            {
                Getter = (context, item) => item._fileInfo.LastWriteTimeUtc,
                Setter = (context, item, value) =>
                {
                    item._fileInfo.LastWriteTimeUtc = value;
                    return HttpStatusCode.OK;
                }
            },
            new DavGetResourceType<EncryptingDiskStoreFile>
            {
                Getter = (context, item) => null
            },

            // Default locking property handling via the LockingManager
            new DavLockDiscoveryDefault<EncryptingDiskStoreFile>(),
            new DavSupportedLockDefault<EncryptingDiskStoreFile>(),

            // Hopmann/Lippert collection properties
            // (although not a collection, the IsHidden property might be valuable)
            new DavExtCollectionIsHidden<EncryptingDiskStoreFile>
            {
                Getter = (context, item) => (item._fileInfo.Attributes & FileAttributes.Hidden) != 0
            },

            // Win32 extensions
            new Win32CreationTime<EncryptingDiskStoreFile>
            {
                Getter = (context, item) => item._fileInfo.CreationTimeUtc,
                Setter = (context, item, value) =>
                {
                    item._fileInfo.CreationTimeUtc = value;
                    return HttpStatusCode.OK;
                }
            },
            new Win32LastAccessTime<EncryptingDiskStoreFile>
            {
                Getter = (context, item) => item._fileInfo.LastAccessTimeUtc,
                Setter = (context, item, value) =>
                {
                    item._fileInfo.LastAccessTimeUtc = value;
                    return HttpStatusCode.OK;
                }
            },
            new Win32LastModifiedTime<EncryptingDiskStoreFile>
            {
                Getter = (context, item) => item._fileInfo.LastWriteTimeUtc,
                Setter = (context, item, value) =>
                {
                    item._fileInfo.LastWriteTimeUtc = value;
                    return HttpStatusCode.OK;
                }
            },
            new Win32FileAttributes<EncryptingDiskStoreFile>
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


        public IPropertyManager PropertyManager => DefaultPropertyManager;
        public ILockingManager LockingManager { get; }

        public async Task<StoreItemResult> CopyAsync(IDavFolder destination, string name, bool overwrite, CancellationToken cancellationToken)
        {
            try
            {
                // If the destination is also a disk-store, then we can use the FileCopy API
                // (it's probably a bit more efficient than copying in C#)
                if (destination is DiskStoreCollection diskCollection)
                {
                    // Check if the collection is writable
                    if (!diskCollection.IsWritable)
                        return new StoreItemResult(HttpStatusCode.Forbidden);

                    var destinationPath = NativePathHelpers.GetCiphertextPath(Path.Combine(diskCollection.Id, name), _specifics);

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
                    IDavFile davFile;
                    try
                    {
                        davFile = (IDavFile)await destination.CreateFileAsync(name, overwrite, cancellationToken).ConfigureAwait(false);
                    }
                    catch (HttpListenerException ex)
                    {
                        return new StoreItemResult((HttpStatusCode)ex.ErrorCode);
                    }

                    // Copy the file content
                    try
                    {
                        await using var sourceStream = await OpenStreamAsync(FileAccess.Read, cancellationToken).ConfigureAwait(false);
                        await using var destinationStream = await davFile.OpenStreamAsync(FileAccess.Write, cancellationToken).ConfigureAwait(false);
                        await sourceStream.CopyToAsync(destinationStream, cancellationToken).ConfigureAwait(false);
                        await destinationStream.FlushAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (IOException ioException) when (ioException.IsDiskFull())
                    {
                        return new StoreItemResult(HttpStatusCode.InsufficientStorage, davFile);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return new StoreItemResult(HttpStatusCode.Forbidden, davFile);
                    }

                    // Return result
                    return new StoreItemResult(HttpStatusCode.Created, davFile);
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

        public override bool Equals(object? obj)
        {
            if (obj is not EncryptingDiskStoreFile storeItem)
                return false;

            return storeItem._fileInfo.FullName.Equals(_fileInfo.FullName, StringComparison.CurrentCultureIgnoreCase);
        }

        private string DetermineContentType()
        {
            return MimeTypeHelper.GetMimeType(Name);
        }
    }
}
