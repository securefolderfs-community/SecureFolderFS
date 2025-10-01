using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Ftp.Extensions;

namespace SecureFolderFS.Sdk.Ftp
{
    public class FtpFolder : FtpStorable, IChildFolder, IModifiableFolder
    {
        public FtpFolder(AsyncFtpClient ftpClient, string id, string name, IFolder? parentFolder = null)
            : base(ftpClient, id, name, parentFolder)
        {
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!await ftpClient.EnsureConnectionAsync(cancellationToken))
                throw FtpExceptions.NotConnectedException;

            await foreach (var item in ftpClient.GetListingEnumerable(Id, cancellationToken, cancellationToken))
            {
                var id = CombinePath(Id, item.Name);
                if (item.Name is "." or "..")
                    continue;

                yield return item.Type switch
                {
                    FtpObjectType.File => new FtpFile(ftpClient, id, item.Name, this),
                    FtpObjectType.Directory => new FtpFolder(ftpClient, id, item.Name, this)
                };
            }
        }

        /// <inheritdoc/>
        public Task<IFolderWatcher> GetFolderWatcherAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromException<IFolderWatcher>(new NotSupportedException());
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(IStorableChild item, CancellationToken cancellationToken = default)
        {
            if (!await ftpClient.EnsureConnectionAsync(cancellationToken))
                throw FtpExceptions.NotConnectedException;

            var id = CombinePath(Id, item.Name);
            switch (item)
            {
                case IFile:
                    await ftpClient.DeleteFile(id, cancellationToken);
                    break;

                case IFolder:
                    await ftpClient.DeleteDirectory(id, cancellationToken);
                    break;

                default:
                    throw new NotSupportedException($"Unsupported type {item.GetType()}.");
            }
        }

        /// <inheritdoc/>
        public async Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            if (!await ftpClient.EnsureConnectionAsync(cancellationToken))
                throw FtpExceptions.NotConnectedException;

            var id = CombinePath(Id, name);
            if (!await ftpClient.CreateDirectory(id, cancellationToken))
                throw new UnauthorizedAccessException("Cannot create folder.");

            return new FtpFolder(ftpClient, id, name, this);
        }

        /// <inheritdoc/>
        public async Task<IChildFile> CreateFileAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            if (!await ftpClient.EnsureConnectionAsync(cancellationToken))
                throw FtpExceptions.NotConnectedException;

            var id = CombinePath(Id, name);
            await using var stream = new MemoryStream();
            await ftpClient.UploadStream(stream, id, token: cancellationToken);

            return new FtpFile(ftpClient, id, name, this);
        }
    }
}
