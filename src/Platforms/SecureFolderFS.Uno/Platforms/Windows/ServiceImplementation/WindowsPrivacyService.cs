using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Uno.Platforms.Windows.ServiceImplementation
{
    /// <inheritdoc cref="IPrivacyService"/>
    internal sealed class WindowsPrivacyService : IPrivacyService
    {
        /// <inheritdoc/>
        public async Task<bool> ClearTracesAsync(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            try
            {
                // Clear Windows Recent Items (Jump Lists and Recent folder)
                ClearRecentDocuments();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Clears the Windows Recent Documents list using Shell32 API.
        /// </summary>
        private static void ClearRecentDocuments()
        {
#if WINDOWS
        // Use SHAddToRecentDocs with null to clear the recent documents list
        // SHARD_PIDL = 0x00000001
        SecureFolderFS.Uno.PInvoke.UnsafeNative.SHAddToRecentDocs(0x00000001, IntPtr.Zero);
#endif
        }
    }
}

