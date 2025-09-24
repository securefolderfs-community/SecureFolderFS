using OwlCore.Storage;

namespace SecureFolderFS.Tests
{
    public sealed record class MockVaultOptions
    {
        public IModifiableFolder? VaultFolder { get; init; }

        public bool IsInitialized { get; }
    }
}
