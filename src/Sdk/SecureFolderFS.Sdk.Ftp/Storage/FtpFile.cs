using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using OwlCore.Storage;

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
            if (!ftpClient.IsConnected)
                throw FtpExceptions.NotConnectedException;

            var size = await ftpClient.GetFileSize(Id, -1L, cancellationToken);
            var stream = accessMode switch
            {
                FileAccess.Read => await ftpClient.OpenRead(Id, token: cancellationToken),
                _ => await ftpClient.OpenWrite(Id, token: cancellationToken)
            };

            return new SeekableFtpStream(stream, size);
        }
    }
}
