using SecureFolderFS.Core.Directories;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.NestedStorage;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Cryptography.Storage
{
    public abstract class CryptoStorable<TCapability> : IWrapper<TCapability>, INestedStorable, IWrappable<IFile>, IWrappable<IFolder>
        where TCapability : IStorable
    {
        protected readonly IStreamsAccess streamsAccess;
        protected readonly IPathConverter pathConverter;
        protected readonly DirectoryIdCache directoryIdCache;

        //protected ICryptoTransform _contentEncryptor;
        //protected ICryptoTransform _contentDecryptor;
        //protected ICryptoTransform _nameEncryptor;
        //protected ICryptoTransform _nameDecryptor;
        //protected IFormatProvider _nameCrypt;

        /// <inheritdoc/>
        public virtual TCapability Inner { get; }

        /// <inheritdoc/>
        public virtual string Id { get; }

        /// <inheritdoc/>
        public virtual string Name { get; }

        protected CryptoStorable(TCapability inner)
        {
            Inner = inner;
        }

        /// <inheritdoc/>
        public virtual async Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            if (Inner is not INestedStorable nestedStorable)
                throw new NotSupportedException("Retrieving the parent folder is not supported.");

            var parent = await nestedStorable.GetParentAsync(cancellationToken);
            if (parent is null)
                return null;

            return (IFolder)Wrap(parent);
        }

        /// <inheritdoc/>
        public virtual IWrapper<IFile> Wrap(IFile file)
        {
            return new CryptoFile(file);
        }

        /// <inheritdoc/>
        public virtual IWrapper<IFolder> Wrap(IFolder folder)
        {
            return new CryptoFolder(folder);
        }

        /// <summary>
        /// Encrypts the provided <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name to encrypt.</param>
        /// <returns>If successful, returns the encrypted name, otherwise empty.</returns>
        protected virtual string EncryptName(string name)
        {
            var ciphertextName = pathConverter.GetCiphertextFileName(System.IO.Path.Combine(Id, name));
            if (ciphertextName is null)
                throw new CryptographicException("Couldn't convert to ciphertext name.");

            return ciphertextName;
        }

        /// <summary>
        /// Decrypts the provided <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name to decrypt.</param>
        /// <returns>If successful, returns the decrypted name, otherwise empty.</returns>
        protected virtual string DecryptName(string name)
        {
            var cleartextName = pathConverter.GetCleartextFileName(System.IO.Path.Combine(Id, name));
            if (cleartextName is null)
                throw new CryptographicException("Couldn't convert to cleartext name.");

            return cleartextName;
        }
    }
}
