using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.Ftp.StorageProperties
{
    /// <inheritdoc cref="ISizeOfProperty"/>
    public sealed class FtpSizeOfProperty : ISizeOfProperty
    {
        private readonly string _id;
        private readonly AsyncFtpClient _ftpClient;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public FtpSizeOfProperty(string id, AsyncFtpClient ftpClient)
        {
            _id = id;
            _ftpClient = ftpClient;
            Name = nameof(ISizeOf.SizeOf);
            Id = $"{id}/{nameof(ISizeOf.SizeOf)}";
        }

        /// <inheritdoc/>
        public async Task<long?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            var size = await _ftpClient.GetFileSize(_id, -1L, cancellationToken);
            if (size == -1L)
                return null;

            return size;
        }
    }
}