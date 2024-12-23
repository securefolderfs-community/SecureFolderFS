using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Helpers.Abstract;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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

            var plaintextName = await DecryptNameAsync(ciphertextParent.Name, ciphertextParentOfParent);
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

        /// <summary>
        /// Encrypts the provided <paramref name="plaintextName"/>.
        /// </summary>
        /// <param name="plaintextName">The name to encrypt.</param>
        /// <param name="parentFolder">The ciphertext parent folder.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is an encrypted name.</returns>
        protected virtual async Task<string> EncryptNameAsync(string plaintextName, IFolder parentFolder)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextName;

            var directoryId = AbstractPathHelpers.AllocateDirectoryId(specifics.Security, plaintextName);
            var result = await AbstractPathHelpers.GetDirectoryIdAsync(parentFolder, specifics, directoryId);

            return specifics.Security.NameCrypt.EncryptName(plaintextName, result ? directoryId : ReadOnlySpan<byte>.Empty) + FileSystem.Constants.Names.ENCRYPTED_FILE_EXTENSION;
        }

        /// <summary>
        /// Decrypts the provided <paramref name="ciphertextName"/>.
        /// </summary>
        /// <param name="ciphertextName">The name to decrypt.</param>
        /// <param name="parentFolder">The ciphertext parent folder.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is a decrypted name.</returns>
        protected virtual async Task<string?> DecryptNameAsync(string ciphertextName, IFolder parentFolder)
        {
            if (specifics.Security.NameCrypt is null)
                return ciphertextName;

            var directoryId = AbstractPathHelpers.AllocateDirectoryId(specifics.Security, ciphertextName);
            var result = await AbstractPathHelpers.GetDirectoryIdAsync(parentFolder, specifics, directoryId);

            return specifics.Security.NameCrypt.DecryptName(Path.GetFileNameWithoutExtension(ciphertextName), result ? directoryId : ReadOnlySpan<byte>.Empty);
        }
    }
}
