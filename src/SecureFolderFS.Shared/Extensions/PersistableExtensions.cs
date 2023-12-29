using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Extensions
{
    /// <summary>
    /// Contains common extensions for <see cref="IPersistable"/>.
    /// </summary>
    public static class PersistableExtensions
    {
        /// <summary>
        /// Tries to asynchronously load persisted data into memory.
        /// </summary>
        /// <param name="persistable">The <see cref="IPersistable"/> instance to use.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns true; otherwise false.</returns>
        public static async Task<bool> TryLoadAsync(this IPersistable persistable, CancellationToken cancellationToken = default)
        {
            try
            {
                await persistable.LoadAsync(cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to asynchronously save data stored in memory.
        /// </summary>
        /// <param name="persistable">The <see cref="IPersistable"/> instance to use.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns true; otherwise false.</returns>
        public static async Task<bool> TrySaveAsync(this IPersistable persistable, CancellationToken cancellationToken = default)
        {
            try
            {
                await persistable.SaveAsync(cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
