using System;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// A model that reports statistics of ongoing vault operations.
    /// </summary>
    public interface IVaultStatisticsModel : IDisposable
    {
        /// <summary>
        /// Sets <paramref name="callback"/> to be notified when bytes are read.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        void NotifyForBytesRead(Action<long> callback);

        /// <summary>
        /// Sets <paramref name="callback"/> to be notified when bytes are written.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        void NotifyForBytesWrite(Action<long> callback);
    }
}
