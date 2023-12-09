using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Utilities
{
    /// <summary>
    /// Represents an authentication interface that provides a method to authenticate entities asynchronously.
    /// </summary>
    public interface IAuthenticator<TAuthentication>
    {
        /// <summary>
        /// Authenticates the entity asynchronously.
        /// </summary>
        /// <param name="id">The ID that uniquely identifies each authentication transaction.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, value is <typeparamref name="TAuthentication"/> that represents the authenticated entity.</returns>
        Task<TAuthentication> AuthenticateAsync(string id, CancellationToken cancellationToken);
    }
}
