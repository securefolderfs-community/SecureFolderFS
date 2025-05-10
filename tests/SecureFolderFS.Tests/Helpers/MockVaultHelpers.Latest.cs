using OwlCore.Storage;

namespace SecureFolderFS.Tests.Helpers
{
    internal static partial class MockVaultHelpers
    {
        public static async Task<(IFolder, string)> CreateVaultLatestAsync(CancellationToken cancellationToken = default)
        {
            return await CreateVaultV3Async(cancellationToken);
        }
    }
}
