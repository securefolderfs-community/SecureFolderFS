using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Represents the result of an OAuth authentication process.
    /// </summary>
    /// <param name="Code">An optional authorization code obtained after successful authentication.</param>
    /// <param name="State">An optional state value used during the authentication process.</param>
    /// <param name="Error">An optional error value if the authentication process encountered an issue.</param>
    public sealed record OAuthResult(object? Code, object? State, object? Error);

    /// <summary>
    /// Represents a handler for OAuth authentication processes.
    /// </summary>
    public interface IOAuthHandler
    {
        /// <summary>
        /// Gets the URL to which the user will be redirected during the OAuth authentication process.
        /// </summary>
        string RedirectUrl { get; }

        /// <summary>
        /// Asynchronously retrieves the OAuth code by navigating to the specified URL
        /// and waits for the user to complete the authentication process.
        /// </summary>
        /// <param name="url">The URL to which the user should be redirected for authentication.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous operation.
        /// Value is an <see cref="IResult{T}"/> with an <see cref="OAuthResult"/> indicating the outcome of the authentication process.
        /// </returns>
        Task<IResult<OAuthResult>> GetCodeAsync(string url, CancellationToken cancellationToken = default);
    }
}
