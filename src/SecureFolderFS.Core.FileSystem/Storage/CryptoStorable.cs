using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Core.FileSystem.Storage
{
    /// <inheritdoc cref="IStorable"/>
    public abstract class CryptoStorable<TCapability> : IWrapper<TCapability>, IStorableChild, IStorableProperties
        where TCapability : IStorable
    {
        protected readonly CryptoFolder? parent;
        protected readonly FileSystemSpecifics specifics;
        protected IBasicProperties? properties;

        /// <inheritdoc/>
        public TCapability Inner { get; }

        /// <inheritdoc/>
        public virtual string Id { get; }

        /// <inheritdoc/>
        public virtual string Name { get; }

        protected CryptoStorable(string plaintextId, TCapability inner, FileSystemSpecifics specifics, CryptoFolder? parent = null)
        {
            this.parent = parent;
            this.specifics = specifics;

            Inner = inner;
            Id = plaintextId;
            Name = Path.GetFileName(plaintextId);
        }

        /// <inheritdoc/>
        public virtual async Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            if (parent is not null)
                return parent;

            if (Inner is not IStorableChild storableChild)
                throw new NotSupportedException("Retrieving the parent folder is not supported.");

            // Make sure we don't go outside the root
            if (storableChild.Id == specifics.ContentFolder.Id || !specifics.ContentFolder.Id.Contains(storableChild.Id))
                return null;

            var ciphertextParent = await storableChild.GetParentAsync(cancellationToken);
            if (ciphertextParent is null)
                return null;

            if (ciphertextParent is not IChildFolder ciphertextParentFolder)
                throw new NotSupportedException("Retrieving the parent folder is not supported.");

            // Get the parent folder of the parent folder to be able to retrieve the DirectoryID
            // If the parent of parent is null, then we can assume we are at the root level and should use ContentFolder
            var ciphertextParentOfParent = await ciphertextParentFolder.GetParentAsync(cancellationToken) ?? specifics.ContentFolder;

            var plaintextName = await AbstractPathHelpers.DecryptNameAsync(ciphertextParent.Name, ciphertextParentOfParent, specifics, cancellationToken);
            if (plaintextName is null)
                return null;

            return (IFolder?)Wrap(ciphertextParent, plaintextName);
        }

        /// <inheritdoc/>
        public abstract Task<IBasicProperties> GetPropertiesAsync();

        protected virtual IWrapper<IFile> Wrap(IFile file, params object[] objects)
        {
            if (objects[0] is not string plaintextName)
                throw new ArgumentException($"The first argument of {nameof(objects)} should be the plaintext name.");

            var plaintextId = Path.Combine(Id, plaintextName);
            return new CryptoFile(plaintextId, file, specifics, this as CryptoFolder);
        }

        protected virtual IWrapper<IFolder> Wrap(IFolder folder, params object[] objects)
        {
            if (objects[0] is not string plaintextName)
                throw new ArgumentException($"The first argument of {nameof(objects)} should be the plaintext name.");

            var plaintextId = Path.Combine(Id, plaintextName);
            return new CryptoFolder(plaintextId, folder, specifics, this as CryptoFolder);
        }
    }
}
