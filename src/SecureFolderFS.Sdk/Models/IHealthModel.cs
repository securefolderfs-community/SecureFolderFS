using SecureFolderFS.Sdk.EventArguments;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Scans the ciphertext folder and reports any problems through <see cref="IssueFound"/> event.
    /// </summary>
    public interface IHealthModel : IDisposable
    {
        /// <summary>
        /// Occurs when an issue is found within the file system structure.
        /// </summary>
        event EventHandler<HealthIssueEventArgs>? IssueFound;

        /// <summary>
        /// Starts the scanning of the file system structure and reports errors through <see cref="IssueFound"/> event.
        /// </summary>
        /// <param name="includeFileContents">Determines whether to include file contents during the scan operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task ScanAsync(bool includeFileContents, CancellationToken cancellationToken = default);
    }
}
