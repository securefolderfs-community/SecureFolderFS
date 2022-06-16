using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Enums;

namespace SecureFolderFS.WinUI.Storage.NativeStorage
{
    /// <inheritdoc cref="IBaseStorage"/>
    internal abstract class NativeBaseStorage : IBaseStorage
    {
        /// <inheritdoc/>
        public string Path { get; protected set; }

        /// <inheritdoc/>
        public string Name { get; protected set; }

        protected NativeBaseStorage(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileName(path);
        }

        /// <inheritdoc/>
        public Task<IFolder?> GetParentAsync()
        {
            var parentPath = System.IO.Path.GetDirectoryName(Path);
            if (string.IsNullOrEmpty(parentPath))
                return Task.FromResult<IFolder?>(null);

            return Task.FromResult<IFolder?>(new NativeFolder(parentPath));
        }

        /// <inheritdoc/>
        public abstract Task<bool> RenameAsync(string newName, NameCollisionOption options);

        /// <inheritdoc/>
        public abstract Task<bool> DeleteAsync(bool permanently);
    }
}
