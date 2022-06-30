namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// A model that reports statistics of ongoing vault operations.
    /// </summary>
    public interface IVaultStatisticsModel
    {
        /// <summary>
        /// Reports the amount of read bytes per operation.
        /// </summary>
        /// <param name="amount">The amount, in bytes.</param>
        void NotifyBytesRead(long amount);

        /// <summary>
        /// Reports the amount of written bytes per operation.
        /// </summary>
        /// <param name="amount">The amount, in bytes.</param>
        void NotifyBytesWritten(long amount);

        // TODO: More statistics can be added as we go
    }
}
