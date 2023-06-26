using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.ViewModels.Controls;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service that interacts with common app related APIs.
    /// </summary>
    public interface IApplicationService
    {
        /// <summary>
        /// Gets the name that uniquely identifies a platform.
        /// </summary>
        string Platform { get; }

        /// <summary>
        /// Gets the version of the app.
        /// </summary>
        /// <returns><see cref="Version"/> of the app.</returns>
        AppVersion GetAppVersion();

        /// <summary>
        /// Gets the version information of the platform that the app is running on.
        /// </summary>
        /// <remarks>
        /// The return value may contain information like OS build version, release number, and other platform-specific members.
        /// </remarks>
        /// <returns>A <see cref="string"/> containing version data.</returns>
        string GetSystemVersion();

        /// <summary>
        /// Launches an URI from app. This can be an URL, folder path, etc.
        /// </summary>
        /// <param name="uri">The URI to launch.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task OpenUriAsync(Uri uri);

        /// <summary>
        /// Retrieves all licenses of packages and libraries that are used by the app.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="LicenseViewModel"/> for licenses.</returns>
        IAsyncEnumerable<LicenseViewModel> GetLicensesAsync(CancellationToken cancellationToken = default);
    }
}
