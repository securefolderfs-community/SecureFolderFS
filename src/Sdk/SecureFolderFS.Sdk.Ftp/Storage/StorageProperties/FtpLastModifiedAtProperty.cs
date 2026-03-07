using System;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.Ftp.StorageProperties
{
    /// <inheritdoc cref="ILastModifiedAtProperty"/>
    public sealed class FtpLastModifiedAtProperty : ILastModifiedAtProperty
    {
        private readonly string _id;
        private readonly AsyncFtpClient _ftpClient;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public FtpLastModifiedAtProperty(string id, AsyncFtpClient ftpClient)
        {
            _id = id;
            _ftpClient = ftpClient;
            Name = nameof(ILastModifiedAt.LastModifiedAt);
            Id = $"{id}/{nameof(ILastModifiedAt.LastModifiedAt)}";
        }

        /// <inheritdoc/>
        public async Task<DateTime?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            var info = await _ftpClient.GetObjectInfo(_id, false, cancellationToken);
            return info.Modified;
        }
    }
}
