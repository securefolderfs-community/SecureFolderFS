using System.Security.Principal;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Http.Context
{
    /// <inheritdoc cref="IHttpSession"/>
    internal sealed class HttpSession : IHttpSession
    {
        /// <inheritdoc/>
        public IPrincipal? Principal { get; private set; }

        public HttpSession(IPrincipal? principal)
        {
            Principal = principal;
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            Principal = null;
            return default;
        }
    }
}
