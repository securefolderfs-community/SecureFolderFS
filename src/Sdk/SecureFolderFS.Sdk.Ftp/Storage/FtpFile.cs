using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Ftp.Extensions;

namespace SecureFolderFS.Sdk.Ftp
{
    public class FtpFile : FtpStorable, IChildFile
    {
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
