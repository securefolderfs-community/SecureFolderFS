using NWebDav.Server.Storage;
using SecureFolderFS.Core.WebDav.Storage.StorageProperties;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.StorageProperties;

namespace SecureFolderFS.Core.WebDav.Storage
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
        public virtual IBasicProperties Properties => InitializeProperties((_propertiesCache ??= new DavStorageProperties<TImplementation>()));

        /// <summary>
        /// Gets the implementor which is represented by <typeparamref name="TImplementation"/> type.
        /// </summary>
        protected abstract TImplementation Implementation { get; }

        protected DavStorable(TStorableInternal storableInternal, IBasicProperties? properties = null)
        {
            StorableInternal = storableInternal;
            _propertiesCache = properties;
        }

        private IBasicProperties InitializeProperties(IBasicProperties properties)
        {
            if (properties is DavStorageProperties<TImplementation> { Storable: null } davStorageProperties)
                davStorageProperties.Storable = Implementation;

            return properties;
        }
    }
}
