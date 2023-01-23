using NWebDav.Server.Storage;
using SecureFolderFS.Core.WebDav.EncryptingStorage;
using SecureFolderFS.Core.WebDav.Storage.StorageProperties;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.StorageProperties;

namespace SecureFolderFS.Core.WebDav.Storage
{
    /// <inheritdoc cref="IDavStorable"/>
    internal abstract class DavStorable<TImplementation, TStorableInternal> : IDavStorableInternal<TStorableInternal>, IInstantiableDavStorage, IDavStorable
        where TStorableInternal : IStorable
        where TImplementation : IDavStorable
    {
        private IBasicProperties? _properties;

        /// <inheritdoc/>
        public TStorableInternal Inner { get; }

        /// <inheritdoc/>
        public virtual string Id => Inner.Id;

        /// <inheritdoc/>
        public virtual string Name => Inner.Name;

        /// <inheritdoc/>
        public virtual IBasicProperties Properties
        {
            get
            {
                _properties ??= new DavStorageProperties<TImplementation>();
                if (_properties is DavStorageProperties<TImplementation> { Storable: null } davStorageProperties)
                    davStorageProperties.Storable = Implementation;

                return _properties;
            }
        }

        /// <summary>
        /// Gets the implementor which is represented by <typeparamref name="TImplementation"/> type.
        /// </summary>
        protected abstract TImplementation Implementation { get; }

        protected DavStorable(TStorableInternal inner, IBasicProperties? properties = null)
        {
            Inner = inner;
            _properties = properties;
        }

        /// <inheritdoc/>
        public virtual DavFile<T> NewFile<T>(T inner)
            where T : IFile
        {
            return new DavFile<T>(inner);
        }

        /// <inheritdoc/>
        public virtual DavFolder<T> NewFolder<T>(T inner)
            where T : IFolder
        {
            return new DavFolder<T>(inner);
        }
    }
}
