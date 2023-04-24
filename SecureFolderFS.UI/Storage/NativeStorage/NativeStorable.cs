using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.Storage.NativeStorage
{
    /// <inheritdoc cref="IStorable"/>
    public abstract class NativeStorable : ILocatableStorable
    {
        /// <inheritdoc/>
        public string Path { get; protected set; }

        /// <inheritdoc/>
        public string Name { get; protected set; }

        /// <inheritdoc/>
        public virtual string Id { get; }

        protected NativeStorable(string path)
        {
            Id = path;
            Path = FormatPath(path);
            Name = System.IO.Path.GetFileName(path);
        }

        /// <inheritdoc/>
        public virtual Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            var parentPath = System.IO.Path.GetDirectoryName(Path);
            if (string.IsNullOrEmpty(parentPath))
                return Task.FromResult<IFolder?>(null);

            return Task.FromResult<IFolder?>(new NativeFolder(parentPath));
        }

        protected static string FormatPath(string path)
        {
            path = path.Replace("file:///", string.Empty);

            if ('/' != System.IO.Path.DirectorySeparatorChar)
                return path.Replace('/', System.IO.Path.DirectorySeparatorChar);

            if ('\\' != System.IO.Path.DirectorySeparatorChar)
                return path.Replace('\\', System.IO.Path.DirectorySeparatorChar);

            return path;
        }
    }
}