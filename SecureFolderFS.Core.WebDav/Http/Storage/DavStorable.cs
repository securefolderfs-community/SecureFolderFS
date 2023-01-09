using NWebDav.Server.Storage;
using SecureFolderFS.Core.WebDav.Http.Storage.StorageProperties;
using SecureFolderFS.Core.WebDav.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.StorageProperties;

namespace SecureFolderFS.Core.WebDav.Http.Storage
{
    /// <inheritdoc cref="IDavStorable"/>
    internal abstract class DavStorable<TImplementation, TStorableInternal> : IDavStorableInternal<TStorableInternal>, IDavStorable
        where TStorableInternal : IStorable
        where TImplementation : IDavStorable
    {
        private IBasicProperties? _propertiesCache;

        /// <inheritdoc/>
        public virtual TStorableInternal StorableInternal { get; }

        /// <inheritdoc/>
        public virtual string Id => StorableInternal.Id;

        /// <inheritdoc/>
        public virtual string Name => StorableInternal.Name;

        /// <inheritdoc/>
        public virtual IBasicProperties Properties => InitializeProperties((_propertiesCache ??= new DavBasicProperties<TImplementation>()));

        /// <summary>
        /// Gets the implementor which is represented by <typeparamref name="TImplementation"/> type.
        /// </summary>
        protected abstract TImplementation Implementation { get; }

        protected DavStorable(TStorableInternal storableInternal, IBasicProperties? properties)
        {
            StorableInternal = storableInternal;
            _propertiesCache = properties;
        }

        private IBasicProperties InitializeProperties(IBasicProperties properties)
        {
            if (properties is DavBasicProperties<TImplementation> { Storable: null } davBasicProperties)
                davBasicProperties.Storable = Implementation;

            return properties;
        }
    }
}
