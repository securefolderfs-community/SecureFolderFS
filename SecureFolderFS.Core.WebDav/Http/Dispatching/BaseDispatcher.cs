using SecureFolderFS.Core.WebDav.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Http.Dispatching
{
    /// <inheritdoc cref="IRequestDispatcher"/>
    internal abstract class BaseDispatcher : IRequestDispatcher
    {
        protected static CommonResult NotImplemented { get; } = new(new NotImplementedException());

        /// <inheritdoc/>
        public IRequestHandlerProvider RequestHandlerProvider { get; }

        protected BaseDispatcher(IRequestHandlerProvider requestHandlerProvider)
        {
            RequestHandlerProvider = requestHandlerProvider;
        }

        /// <inheritdoc/>
        public virtual async Task<IResult> DispatchAsync(IHttpContext context, CancellationToken cancellationToken = default)
        {
            if (!RequestHandlerProvider.RequestHandlers.TryGetValue(context.Request.HttpMethod, out var requestHandler))
            {
                context.Response.SetStatus(HttpStatusCode.NotImplemented);
                return NotImplemented;
            }

            try
            {
                // TODO: Maybe return IResult<HttpStatusCode> and set the response status based on the result
                await InvokeRequestAsync(requestHandler, context, cancellationToken);

                return CommonResult.Success;
            }
            catch (Exception ex)
            {
                context.Response.SetStatus(HttpStatusCode.InternalServerError);
                return new CommonResult(ex);
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        protected abstract Task InvokeRequestAsync(IRequestHandler requestHandler, IHttpContext context, CancellationToken cancellationToken);

        /// <inheritdoc/>
        public virtual void Dispose()
        {
        }
    }
}
