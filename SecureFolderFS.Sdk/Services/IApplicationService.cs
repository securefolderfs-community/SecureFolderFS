using System;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.AppModels;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service that interacts with common app related APIs.
    /// </summary>
    public interface IApplicationService
    {
        /// <summary>
        /// Gets the version of the app.
        /// </summary>
        /// <returns><see cref="Version"/> of the app.</returns>
        AppVersion GetAppVersion();

        /// <summary>
        /// Launches an URI from app. This can be an URL, folder path, etc.
        /// </summary>
        /// <param name="uri">The URI to launch.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task OpenUriAsync(Uri uri);
    }
}
