using FluentFTP;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.Ftp
{
    public abstract class FtpStorable : IStorableChild
    {
        protected readonly AsyncFtpClient _ftpClient;
        protected readonly IFolder? _parentFolder;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        protected FtpStorable(AsyncFtpClient ftpClient, string id, string name, IFolder? parentFolder = null)
        {
            _ftpClient = ftpClient;
            _parentFolder = parentFolder;
            Id = id;
            Name = name;
        }

        /// <inheritdoc/>
        public Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_parentFolder);
        }

        protected static string CombinePath(string path1, string path2)
        {
            if (path1.EndsWith('/'))
                return path1 + path2;

            return $"{path1}/{path2}";
        }
    }
}
