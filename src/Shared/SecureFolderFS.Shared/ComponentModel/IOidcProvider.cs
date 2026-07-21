using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Provides OAuth2/OIDC authentication to obtain access tokens for a server.
    /// </summary>
    public interface IOidcProvider
    {
        /// <summary>
        /// Authenticates the user and returns a valid access token.
        /// </summary>
        /// <param name="authority">The OIDC authority URL.</param>
        /// <param name="clientId">The OAuth2 client ID.</param>
        /// <param name="scopes">The OAuth2 scopes to request.</param>
        /// <param name="forceLogin">When true, forces the identity provider to prompt for login instead of silently reusing an existing SSO session.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is a valid Bearer access token.</returns>
        Task<string> GetAccessTokenAsync(string authority, string clientId, IReadOnlyList<string> scopes, bool forceLogin = false, CancellationToken cancellationToken = default);
    }
}
