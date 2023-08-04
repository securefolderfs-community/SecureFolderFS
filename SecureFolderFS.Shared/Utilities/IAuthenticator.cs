using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Utilities
{
    /// <summary>
    /// Represents an authentication interface that provides a method to authenticate entities asynchronously.
    /// </summary>
    public interface IAuthenticator
    {
        /// <summary>
        /// Authenticates the entity asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, value is <see cref="IIdentity"/> that represents the authenticated entity.</returns>
        Task<IIdentity> AuthenticateAsync(CancellationToken cancellationToken);
    }
}
