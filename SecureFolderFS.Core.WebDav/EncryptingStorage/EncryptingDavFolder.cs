using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.WebDav.Storage;
using SecureFolderFS.Sdk.Storage;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage
{
    internal sealed class EncryptingDavFolder<TCapability> : DavFolder<TCapability>
        where TCapability : IFolder
    {
        private readonly IStreamsAccess _streamsAccess;
        private readonly IPathConverter _pathConverter;

        /// <inheritdoc/>
        public override string Path => _pathConverter.ToCleartext(base.Path) ?? string.Empty;

        public EncryptingDavFolder(TCapability inner, IStreamsAccess streamsAccess, IPathConverter pathConverter)
            : base(inner)
        {
            _streamsAccess = streamsAccess;
            _pathConverter = pathConverter;
        }

        /// <inheritdoc/>
        public override DavFile<T> NewFile<T>(T inner)
        {
            return new EncryptingDavFile<T>(inner, _streamsAccess, _pathConverter);
        }

        /// <inheritdoc/>
        public override DavFolder<T> NewFolder<T>(T inner)
        {
            return new EncryptingDavFolder<T>(inner, _streamsAccess, _pathConverter);
        }

        /// <inheritdoc/>
        protected override string FormatName(string name)
        {
            var cleartextPath = System.IO.Path.Combine(Path, name);
            return _pathConverter.GetCiphertextFileName(cleartextPath) ?? throw new CryptographicException("Couldn't convert to ciphertext path.");;
        }
    }
}
