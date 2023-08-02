using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Utilities
{
    /// <summary>
    /// Provides 
    /// </summary>
    public interface IAuthenticator
    {
        /// <summary>
        /// Authenticates the user to 
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IIdentity"/> that represents the authenticated object.</returns>
        Task<IIdentity> AuthenticateAsync(CancellationToken cancellationToken);
    }
}
