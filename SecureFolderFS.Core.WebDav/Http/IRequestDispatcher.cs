using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Http
{
    // TODO: Needs docs
    internal interface IRequestDispatcher : IDisposable
    {
        IRequestHandlerProvider RequestHandlerProvider { get; }

        Task<IResult> DispatchAsync(IHttpContext context, CancellationToken cancellationToken = default);
    }
}
