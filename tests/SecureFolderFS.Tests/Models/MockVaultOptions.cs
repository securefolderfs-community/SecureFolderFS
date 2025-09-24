using OwlCore.Storage;

namespace SecureFolderFS.Tests.Models
{
    public sealed record class MockVaultOptions
    {
        public IModifiableFolder? VaultFolder { get; init; }

        public bool IsInitialized { get; }
    }
}
