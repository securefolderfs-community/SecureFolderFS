using NWebDav.Server.Storage;
using SecureFolderFS.Core.WebDav.EncryptingStorage;
using SecureFolderFS.Core.WebDav.Storage.StorageProperties;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.NestedStorage;
using SecureFolderFS.Sdk.Storage.StorageProperties;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Storage
{
    /// <inheritdoc cref="IDavStorable"/>
    internal abstract class DavStorable<TImplementation, TCapability> : IDavStorable, IInstantiableDavStorage, IWrapper<TCapability>, IWrappable<IFile>, IWrappable<IFolder>
        where TCapability : IStorable
        where TImplementation : IDavStorable
    {
        protected IBasicProperties? properties;

        /// <inheritdoc/>
        public TCapability Inner { get; }

        /// <inheritdoc/>
        public virtual string Id => Inner.Id;

        /// <inheritdoc/>
        public virtual string Name => Inner.Name;

        /// <inheritdoc/>
        public virtual IBasicProperties Properties
        {
            get
            {
                properties ??= new DavStorageProperties<TImplementation>();
                if (properties is DavStorageProperties<TImplementation> { Storable: null } davStorageProperties)
                    davStorageProperties.Storable = Implementation;

                return properties;
            }
        }

        /// <summary>
        /// Gets the implementor which is represented by <typeparamref name="TImplementation"/> type.
        /// </summary>
        protected abstract TImplementation Implementation { get; }

        protected DavStorable(TCapability inner, IBasicProperties? properties = null)
        {
            Inner = inner;
            this.properties = properties;
        }

        /// <inheritdoc/>
        public virtual async Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            if (Inner is not INestedStorable nestedStorable)
                throw new NotSupportedException("Retrieving the parent folder is not supported.");

            var parent = await nestedStorable.GetParentAsync(cancellationToken);
            if (parent is null)
                return null;

            return NewFolder(parent);
        }

        /// <inheritdoc/>
        public IDavFile NewFile<T>(T inner)
            where T : IFile
        {
            return (IDavFile)Wrap(inner);
        }

        /// <inheritdoc/>
        public IDavFolder NewFolder<T>(T inner)
            where T : IFolder
        {
            return (IDavFolder)Wrap(inner);
        }

        /// <inheritdoc/>
        public abstract IWrapper<IFile> Wrap(IFile file);

        /// <inheritdoc/>
        public abstract IWrapper<IFolder> Wrap(IFolder folder);
    }
}
