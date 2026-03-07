using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Ftp.Extensions;
using SecureFolderFS.Sdk.Ftp.StorageProperties;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.Ftp.Storage
{
    public class FtpFile : FtpStorable, IChildFile, ICreatedAt, ILastModifiedAt, ISizeOf
    {
        /// <inheritdoc/>
        public ICreatedAtProperty CreatedAt => field ??= new FtpCreatedAtProperty(Id, ftpClient);

        /// <inheritdoc/>
        public ILastModifiedAtProperty LastModifiedAt => field ??= new FtpLastModifiedAtProperty(Id, ftpClient);

        /// <inheritdoc/>
        public ISizeOfProperty SizeOf => field ??= new FtpSizeOfProperty(Id, ftpClient);

        public FtpFile(AsyncFtpClient ftpClient, string id, string name, IFolder? parentFolder = null)
            : base(ftpClient, id, name, parentFolder)
        {
        }

        /// <inheritdoc/>
        public async Task<Stream> OpenStreamAsync(FileAccess accessMode, CancellationToken cancellationToken = default)
        {
            if (!await ftpClient.EnsureConnectionAsync(cancellationToken))
                throw FtpExceptions.NotConnectedException;

            var ftpStream = accessMode switch
            {
                FileAccess.Read => await ftpClient.OpenRead(Id, token: cancellationToken),
                FileAccess.Write => await ftpClient.OpenWrite(Id, FtpDataType.Binary, false, token: cancellationToken),
                _ => throw new NotSupportedException($"The {nameof(FileAccess)} '{accessMode}' is not supported on an FTP stream."),
            };

            return ftpStream;
        }
    }
}
