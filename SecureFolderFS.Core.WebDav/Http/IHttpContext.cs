using System;

namespace SecureFolderFS.Core.WebDav.Http
{
    internal interface IHttpContext : IAsyncDisposable
    {
        IHttpRequest Request { get; }

        IHttpResponse Response { get; }

        IHttpSession Session { get; }
    }
}
