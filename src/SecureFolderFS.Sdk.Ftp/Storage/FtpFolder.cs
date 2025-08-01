using System.Runtime.CompilerServices;
using FluentFTP;
using OwlCore.Storage;

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
            if (!_ftpClient.IsConnected)
                throw FtpExceptions.NotConnectedException;

            await foreach (var item in _ftpClient.GetListingEnumerable(Id, cancellationToken, cancellationToken))
            {
                var id = CombinePath(Id, item.Name);
                if (item.Name is "." or "..")
                    continue;

                yield return item.Type switch
                {
                    FtpObjectType.File => new FtpFile(_ftpClient, id, item.Name, this),
                    FtpObjectType.Directory => new FtpFolder(_ftpClient, id, item.Name, this)
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
            if (!_ftpClient.IsConnected)
                throw FtpExceptions.NotConnectedException;

            var id = CombinePath(Id, item.Name);
            switch (item)
            {
                case IFile:
                    await _ftpClient.DeleteFile(id, cancellationToken);
                    break;

                case IFolder:
                    await _ftpClient.DeleteDirectory(id, cancellationToken);
                    break;

                default:
                    throw new NotSupportedException($"Unsupported type {item.GetType()}.");
            }
        }

        /// <inheritdoc/>
        public async Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            if (!_ftpClient.IsConnected)
                throw FtpExceptions.NotConnectedException;

            var id = CombinePath(Id, name);
            if (!await _ftpClient.CreateDirectory(id, cancellationToken))
                throw new UnauthorizedAccessException("Cannot create folder.");

            return new FtpFolder(_ftpClient, id, name, this);
        }

        /// <inheritdoc/>
        public async Task<IChildFile> CreateFileAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            if (!_ftpClient.IsConnected)
                throw FtpExceptions.NotConnectedException;

            var id = CombinePath(Id, name);
            await using var stream = new MemoryStream();
            await _ftpClient.UploadStream(stream, id, token: cancellationToken);

            return new FtpFile(_ftpClient, id, name, this);
        }
    }
}
