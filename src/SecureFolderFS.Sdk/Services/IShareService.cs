using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// Provides functionality for sharing content using the platform's native sharing capabilities.
    /// </summary>
    public interface IShareService
    {
        /// <summary>
        /// Shares a text string along with a title using the platform's sharing functionality.
        /// </summary>
        /// <param name="text">The text content to be shared.</param>
        /// <param name="title">The title associated with the shared text.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task ShareTextAsync(string text, string title);
    }
}
