using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Utilities
{
    /// <summary>
    /// Represents an authentication interface that provides a method to authenticate entities asynchronously.
    /// </summary>
    /// <typeparam name="TAuthentication">The type of the authentication.</typeparam>
    public interface IAuthenticator<TAuthentication>
    {
        /// <summary>
        /// Creates a new authentication for the user.
        /// </summary>
        /// <param name="id">The ID that uniquely identifies each authentication transaction.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, value is <typeparamref name="TAuthentication"/> that represents the authentication.</returns>
        Task<TAuthentication> CreateAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Authenticates the user asynchronously.
        /// </summary>
        /// <param name="id">The ID that uniquely identifies each authentication transaction.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, value is <typeparamref name="TAuthentication"/> that represents the authentication.</returns>
        Task<TAuthentication> AuthenticateAsync(string id, CancellationToken cancellationToken);
    }
}
