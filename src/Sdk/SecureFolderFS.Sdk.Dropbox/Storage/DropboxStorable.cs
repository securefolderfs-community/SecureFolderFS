using Dropbox.Api;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.Dropbox.Storage
{
    public abstract class DropboxStorable : IStorableChild
    {
        protected IFolder? ParentFolder { get; }

        protected DropboxClient Client { get; }

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        protected DropboxStorable(DropboxClient client, string id, string name, IFolder? parent = null)
        {
            Client = client;
            Id = id;
            Name = name;
            ParentFolder = parent;
        }

        /// <inheritdoc/>
        public virtual Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ParentFolder);
        }

        /// <summary>
        /// Builds the Dropbox path for a direct child of this folder.
        /// </summary>
        protected static string CombinePaths(string parentPath, string childName)
        {
            return parentPath == string.Empty ? $"/{childName}" : $"{parentPath}/{childName}";
        }
    }
}