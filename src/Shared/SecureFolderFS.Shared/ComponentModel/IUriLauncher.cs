using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Defines a contract for redirecting or opening a URI.
    /// </summary>
    public interface IUriLauncher
    {
        /// <summary>
        /// Launches a URI from app. This can be a URL, folder path, etc.
        /// </summary>
        /// <param name="uri">The URI to launch.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task OpenUriAsync(Uri uri);
    }
}