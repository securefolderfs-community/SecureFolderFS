using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Ftp.StorageProperties;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.Ftp
{
    public abstract class FtpStorable : IStorableChild, IStorableProperties
    {
        protected readonly AsyncFtpClient ftpClient;
        protected readonly IFolder? parentFolder;
        protected IBasicProperties? properties;

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

        /// <inheritdoc/>
        public Task<IBasicProperties> GetPropertiesAsync()
        {
            properties ??= new FtpItemProperties(Id, ftpClient);
            return Task.FromResult(properties);
        }

        protected static string CombinePath(string path1, string path2)
        {
            if (path1.EndsWith('/'))
                return path1 + path2;

            return $"{path1}/{path2}";
        }
    }
}
