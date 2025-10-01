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

            return accessMode switch
            {
                FileAccess.Read => await ftpClient.OpenRead(Id, token: cancellationToken),
                FileAccess.Write => await ftpClient.OpenWrite(Id, token: cancellationToken),
                FileAccess.ReadWrite => new SeekableFtpStream(await ftpClient.OpenWrite(Id, token: cancellationToken), await ftpClient.GetFileSize(Id, -1L, cancellationToken))
            };
        }
    }
}
