using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.Ftp.Storage
{
    public abstract class FtpStorable : IStorableChild
    {
        protected readonly AsyncFtpClient ftpClient;
        protected readonly IFolder? parentFolder;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        protected FtpStorable(AsyncFtpClient ftpClient, string id, string name, IFolder? parentFolder = null)
        {
            this.ftpClient = ftpClient;
            this.parentFolder = parentFolder;
            Id = id;
            Name = name;
        }

        /// <inheritdoc/>
        public Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(parentFolder);
        }

        protected static string CombinePath(string path1, string path2)
        {
            if (path1.EndsWith('/'))
                return path1 + path2;

            return $"{path1}/{path2}";
        }
    }
}
