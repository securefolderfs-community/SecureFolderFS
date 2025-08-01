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
            if (!_ftpClient.IsConnected)
                throw FtpExceptions.NotConnectedException;

            return accessMode switch
            {
                FileAccess.Read => await _ftpClient.OpenRead(Id, token: cancellationToken),
                _ => await _ftpClient.OpenWrite(Id, token: cancellationToken)
            };
        }
    }
}
