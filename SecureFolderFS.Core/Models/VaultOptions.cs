using SecureFolderFS.Core.Cryptography.Enums;

namespace SecureFolderFS.Core.Models
{
    // TODO: Needs docs
    public sealed class VaultOptions
    {
        public ContentCipherScheme ContentCipherScheme { get; }

        public FileNameCipherScheme FileNameCipherScheme { get; }

        public VaultOptions(ContentCipherScheme contentCipherScheme, FileNameCipherScheme fileNameCipherScheme)
        {
            ContentCipherScheme = contentCipherScheme;
            FileNameCipherScheme = fileNameCipherScheme;
        }
    }
}
