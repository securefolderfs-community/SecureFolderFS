using Dropbox.Api;
using OwlCore.Storage;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.Dropbox.Storage
{
    public abstract class DropboxStorable : IStorableChild, IStorableProperties
    {
        protected IBasicProperties? properties;

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

        /// <inheritdoc/>
        public abstract Task<IBasicProperties> GetPropertiesAsync();

        /// <summary>
        /// Builds the Dropbox path for a direct child of this folder.
        /// <list type="bullet">
        ///   <item>Root (<c>""</c>) + <c>"Photos"</c> → <c>"/Photos"</c></item>
        ///   <item><c>"/Photos"</c> + <c>"vacation.jpg"</c> → <c>"/Photos/vacation.jpg"</c></item>
        /// </list>
        /// </summary>
        protected static string CombinePaths(string parentPath, string childName)
            => parentPath == string.Empty ? $"/{childName}" : $"{parentPath}/{childName}";
    }
}