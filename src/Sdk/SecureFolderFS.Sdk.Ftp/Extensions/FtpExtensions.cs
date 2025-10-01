using System.Threading;
using System.Threading.Tasks;
using FluentFTP;

namespace SecureFolderFS.Sdk.Ftp.Extensions
{
    public static class FtpExtensions
    {
        public static async Task<bool> EnsureConnectionAsync(this AsyncFtpClient ftpClient, CancellationToken cancellationToken = default)
        {
            if (!ftpClient.IsConnected)
                await ftpClient.Connect(cancellationToken);

            return ftpClient.IsConnected;
        }
    }
}
