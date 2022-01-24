using SecureFolderFS.Core.Security.ContentCrypt.FileContent;
using SecureFolderFS.Core.Security.ContentCrypt.FileHeader;
using SecureFolderFS.Core.Security.ContentCrypt.FileName;

namespace SecureFolderFS.Core.Security.ContentCrypt
{
    internal sealed class ContentCryptor : IContentCryptor
    {
        public IFileContentCryptor FileContentCryptor { get; }

        public IFileHeaderCryptor FileHeaderCryptor { get; }

        public IFileNameCryptor FileNameCryptor { get; }

        public ContentCryptor(IFileContentCryptor fileContentCryptor, IFileHeaderCryptor fileHeaderCryptor, IFileNameCryptor fileNameCryptor)
        {
            this.FileContentCryptor = fileContentCryptor;
            this.FileHeaderCryptor = fileHeaderCryptor;
            this.FileNameCryptor = fileNameCryptor;
        }

        public void Dispose()
        {
            FileContentCryptor?.Dispose();
            FileHeaderCryptor?.Dispose();
            FileNameCryptor?.Dispose();
        }
    }
}
