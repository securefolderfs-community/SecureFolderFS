using SecureFolderFS.Core.Cryptography.Enums;

namespace SecureFolderFS.Core.Models
{
    // TODO: Needs docs
    public sealed class VaultOptions
    {
        public required ContentCipherScheme ContentCipher { get; init; }

        public required FileNameCipherScheme FileNameCipher { get; init; }
    }
}
