using OwlCore.Storage;
using SecureFolderFS.Tests.Models;

namespace SecureFolderFS.Tests.Helpers
{
    internal static partial class MockVaultHelpers
    {
        public static async Task<(IFolder, string)> CreateVaultLatestAsync(MockVaultOptions? options, CancellationToken cancellationToken = default)
        {
            return await CreateVaultV3Async(options, cancellationToken);
        }
    }
}
