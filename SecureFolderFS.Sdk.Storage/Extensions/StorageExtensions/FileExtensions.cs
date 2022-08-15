using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage.Extensions
{
    public static partial class StorageExtensions
    {
        /// <returns>If successful, returns a <see cref="Stream"/>, otherwise null.</returns>
        /// <inheritdoc cref="IFile.OpenStreamAsync"/>
        public static async Task<Stream?> TryOpenStreamAsync(this IFile file, FileAccess access, FileShare share = FileShare.None, CancellationToken cancellationToken = default)
        {
            try
            {
                return await file.OpenStreamAsync(access, share, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
