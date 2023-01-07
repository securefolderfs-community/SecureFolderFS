using SecureFolderFS.Sdk.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Http.Dispatching
{
    /// <inheritdoc cref="IDavDispatcher"/>
    internal sealed class WebDavDispatcher : BaseDispatcher
    {
        private readonly IStorageService _davStorageService;

        public WebDavDispatcher(IStorageService davStorageService, IRequestHandlerProvider requestHandlerProvider)
            : base(requestHandlerProvider)
        {
            _davStorageService = davStorageService;
        }

        /// <inheritdoc/>
        protected override async Task InvokeRequestAsync(IRequestHandler requestHandler, IHttpContext context, CancellationToken cancellationToken)
        {
            await requestHandler.ProcessRequestAsync(context, _davStorageService, cancellationToken);
        }
    }
}
