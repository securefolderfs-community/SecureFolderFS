using OwlCore.Storage;

namespace SecureFolderFS.Tests.Extensions
{
    internal static class MockFileExtensions
    {
        public static async Task ResetMockFilePosition(this IFile file, CancellationToken cancellationToken = default)
        {
            await using var stream = await file.OpenReadAsync(cancellationToken);
            stream.Seek(0L, SeekOrigin.Begin);
        }
    }
}
