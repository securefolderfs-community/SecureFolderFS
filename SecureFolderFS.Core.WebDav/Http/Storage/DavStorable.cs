using SecureFolderFS.Core.WebDav.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.StorageProperties;

namespace SecureFolderFS.Core.WebDav.Http.Storage
{
    /// <inheritdoc cref="IDavStorable"/>
    internal abstract class DavStorable<TStorableInternal> : IDavStorableInternal<TStorableInternal>, IDavStorable
        where TStorableInternal : IStorable
    {
        /// <inheritdoc/>
        public TStorableInternal StorableInternal { get; }

        /// <inheritdoc/>
        public virtual string Id => StorableInternal.Id;

        /// <inheritdoc/>
        public virtual string Name => StorableInternal.Name;

        /// <inheritdoc/>
        public IBasicProperties Properties { get; }

        protected DavStorable(TStorableInternal storableInternal, IBasicProperties properties)
        {
            StorableInternal = storableInternal;
            Properties = properties;
        }
    }
}
