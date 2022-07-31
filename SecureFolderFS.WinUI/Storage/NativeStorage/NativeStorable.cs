using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;

namespace SecureFolderFS.WinUI.Storage.NativeStorage
{
    /// <inheritdoc cref="IStorable"/>
    internal abstract class NativeStorable : ILocatableStorable
    {
        /// <inheritdoc/>
        public string Path { get; protected set; }

        /// <inheritdoc/>
        public string Name { get; protected set; }

        /// <inheritdoc/>
        public string Id { get; protected set; }

        protected NativeStorable(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileName(path);
            Id = string.Empty;
        }

        /// <inheritdoc/>
        public virtual Task<ILocatableFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            var parentPath = System.IO.Path.GetDirectoryName(Path);
            if (string.IsNullOrEmpty(parentPath))
                return Task.FromResult<ILocatableFolder?>(null);

            return Task.FromResult<ILocatableFolder?>(new NativeFolder(parentPath));
        }
    }
}
