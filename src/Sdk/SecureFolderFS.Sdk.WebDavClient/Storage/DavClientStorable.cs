using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using WebDav;

namespace SecureFolderFS.Sdk.WebDavClient.Storage
{
    public abstract class DavClientStorable : IStorableChild
    {
        protected readonly IWebDavClient davClient;
        protected readonly HttpClient httpClient;
        protected readonly Uri baseUri;
        protected readonly IFolder? parentFolder;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        protected DavClientStorable(IWebDavClient davClient, HttpClient httpClient, Uri baseUri, string id, string name, IFolder? parentFolder = null)
        {
            this.davClient = davClient;
            this.httpClient = httpClient;
            this.baseUri = baseUri;
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

        protected Uri ResolveUri(string path)
        {
            return new Uri(baseUri, path);
        }
    }
}
