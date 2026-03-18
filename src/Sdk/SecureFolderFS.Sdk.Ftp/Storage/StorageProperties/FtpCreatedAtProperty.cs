using System;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.Ftp.StorageProperties
{
    /// <inheritdoc cref="ICreatedAtProperty"/>
    public sealed class FtpCreatedAtProperty : ICreatedAtProperty
    {
        private readonly string _id;
        private readonly AsyncFtpClient _ftpClient;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public FtpCreatedAtProperty(string id, AsyncFtpClient ftpClient)
        {
            _id = id;
            _ftpClient = ftpClient;
            Name = nameof(ILastModifiedAt.LastModifiedAt);
            Id = $"{id}/{nameof(ILastModifiedAt.LastModifiedAt)}";
        }

        /// <inheritdoc/>
        public async Task<DateTime?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            var info = await _ftpClient.GetObjectInfo(_id, true, cancellationToken);
            return info.Created;
        }
    }
}
