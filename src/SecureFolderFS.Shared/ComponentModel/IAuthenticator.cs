using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Represents an authentication interface that provides a method to authenticate entities asynchronously.
    /// </summary>
    public interface IAuthenticator
    {
        /// <summary>
        /// Removes any associated authentication profiles for the provided <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The ID that uniquely identifies each authentication transaction.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task RevokeAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new authentication for the user.
        /// </summary>
        /// <param name="id">The ID that uniquely identifies each authentication transaction.</param>
        /// <param name="data">The optional data to sign.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, value is <see cref="IKey"/> that represents the authentication.</returns>
        Task<IKey> CreateAsync(string id, byte[]? data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Authenticates the user asynchronously.
        /// </summary>
        /// <param name="id">The ID that uniquely identifies each authentication transaction.</param>
        /// <param name="data">The optional data to sign.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, value is <see cref="IKey"/> that represents the authentication.</returns>
        Task<IKey> SignAsync(string id, byte[]? data, CancellationToken cancellationToken = default);
    }
}
