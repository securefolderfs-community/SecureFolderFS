using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Storage
{
    public abstract class CryptoStorable<TCapability> : IWrapper<TCapability>, IStorableChild, IWrappable<IFile>, IWrappable<IFolder>
        where TCapability : IStorable
    {
        private string? _computedId;
        private string? _computedName;

        protected readonly FileSystemSpecifics specifics;

        /// <inheritdoc/>
        public TCapability Inner { get; }

        /// <inheritdoc/>
        public virtual string Id => _computedId ??= DecryptPath(Inner.Id);

        /// <inheritdoc/>
        public virtual string Name => _computedName ??= DecryptName(Inner.Name);

        protected CryptoStorable(TCapability inner, FileSystemSpecifics specifics)
        {
            this.Inner = inner;
            this.specifics = specifics;
        }

        /// <inheritdoc/>
        public virtual async Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            if (Inner is not IStorableChild storableChild)
                throw new NotSupportedException("Retrieving the parent folder is not supported.");

            // Make sure we don't go outside the root
            if (storableChild.Id == specifics.ContentFolder.Id || !specifics.ContentFolder.Id.Contains(storableChild.Id))
                return null;

            var parent = await storableChild.GetParentAsync(cancellationToken);
            if (parent is null)
                return null;

            return (IFolder?)Wrap(parent);
        }

        /// <inheritdoc/>
        public virtual IWrapper<IFile> Wrap(IFile file)
        {
            return new CryptoFile(file, specifics);
        }

        /// <inheritdoc/>
        public virtual IWrapper<IFolder> Wrap(IFolder folder)
        {
            return new CryptoFolder(folder, specifics);
        }

        /// <summary>
        /// Encrypts the provided <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to encrypt.</param>
        /// <returns>An encrypted path equivalent found on the file system.</returns>
        protected virtual string EncryptPath(string path)
        {
            return path;

            //var ciphertextPath = pathConverter.ToCiphertext(path);
            //if (ciphertextPath is null)
            //    throw new CryptographicException("Couldn't convert to ciphertext path.");

            //return ciphertextPath;
        }

        /// <summary>
        /// Decrypts the provided <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to decrypt.</param>
        /// <returns>A decrypted path version of the one found on the file system.</returns>
        protected virtual string DecryptPath(string path)
        {
            return path;

            //var cleartextPath = pathConverter.ToCleartext(path);
            //if (cleartextPath is null)
            //    throw new CryptographicException("Couldn't convert to cleartext path.");

            //return cleartextPath;
        }

        /// <summary>
        /// Encrypts the provided <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name to encrypt.</param>
        /// <returns>An encrypted name.</returns>
        protected virtual string EncryptName(string name)
        {
            return name;

            //var ciphertextName = pathConverter.GetCiphertextFileName(System.IO.Path.Combine(Id, name));
            //if (ciphertextName is null)
            //    throw new CryptographicException("Couldn't convert to ciphertext name.");

            //return ciphertextName;
        }

        /// <summary>
        /// Decrypts the provided <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name to decrypt.</param>
        /// <returns>A decrypted name.</returns>
        protected virtual string DecryptName(string name)
        {
            return name;

            //var cleartextName = pathConverter.GetCleartextFileName(System.IO.Path.Combine(Id, name));
            //if (cleartextName is null)
            //    throw new CryptographicException("Couldn't convert to cleartext name.");

            //return cleartextName;
        }
    }
}
