using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service to consume app related APIs.
    /// </summary>
    public interface IApplicationService
    {
        /// <summary>
        /// Gets the version of the app.
        /// </summary>
        /// <returns>Version of the app.</returns>
        Version GetAppVersion();

        /// <summary>
        /// Launches any URI from app. This can be an URL, folder path, etc.
        /// </summary>
        /// <param name="uri">The URI to launch.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task OpenUriAsync(Uri uri);
    }
}
