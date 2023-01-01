using SecureFolderFS.Sdk.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Http.Requests
{
    /// <inheritdoc cref="IRequestHandler"/>
    internal sealed class PropPatchHandler : IRequestHandler
    {
        /// <inheritdoc/>
        public Task ProcessRequestAsync(IHttpContext context, IStorageService storageService, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
