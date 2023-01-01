using SecureFolderFS.Core.WebDav.Extensions;
using SecureFolderFS.Sdk.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Http.Requests
{
    /// <inheritdoc cref="IRequestHandler"/>
    internal sealed class OptionsHandler : IRequestHandler
    {
        private readonly string[] _supportedRequests;

        public OptionsHandler(IEnumerable<string> supportedRequests)
        {
            _supportedRequests = supportedRequests.ToArray();
        }

        /// <inheritdoc/>
        public Task ProcessRequestAsync(IHttpContext context, IStorageService storageService, CancellationToken cancellationToken = default)
        {
            context.Response.SetHeaderValue("Dav", Constants.WebDavOptions.DAV_COMPLIANCE_LEVEL);
            context.Response.SetHeaderValue("Allow", string.Join(", ", _supportedRequests));
            context.Response.SetHeaderValue("Public", string.Join(", ", _supportedRequests));

            context.Response.SetStatus(HttpStatusCode.OK);
            return Task.CompletedTask;
        }
    }
}
