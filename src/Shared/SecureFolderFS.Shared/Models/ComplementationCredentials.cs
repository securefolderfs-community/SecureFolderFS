using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Shared.Models
{
    public sealed record class ComplementationCredentials
    {
        public IKeyUsage? CurrentKeystoreCredential { get; init; }

        public IKeyUsage? CurrentPrimaryCredential { get; init; }

        public IKeyUsage? CurrentComplementCredential { get; init; }

        public IKeyUsage? NewPrimaryCredential { get; init; }

        public IKeyUsage? NewComplementCredential { get; init; }
    }
}
