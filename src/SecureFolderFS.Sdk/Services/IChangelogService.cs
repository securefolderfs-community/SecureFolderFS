using SecureFolderFS.Sdk.DataModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// Provides functionality to retrieve changelog information for the application.
    /// </summary>
    public interface IChangelogService
    {
        /// <summary>
        /// Retrieves the latest changelog entry for the specified version.
        /// </summary>
        /// <param name="version">The version of the application to retrieve the changelog for.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the latest <see cref="ChangelogDataModel"/>.</returns>
        Task<ChangelogDataModel> GetLatestAsync(Version version, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all changelog entries since the specified version.
        /// </summary>
        /// <param name="version">The version of the application to start retrieving changelogs from.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="ChangelogDataModel"/> of the changelog entries.</returns>
        IAsyncEnumerable<ChangelogDataModel> GetSinceAsync(Version version, CancellationToken cancellationToken = default);
    }
}
