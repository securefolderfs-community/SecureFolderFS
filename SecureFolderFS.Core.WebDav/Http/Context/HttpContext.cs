using SecureFolderFS.Core.WebDav.Http.HttpListener;
using System.Net;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Http.Context
{
    internal sealed class HttpContext : IHttpContext
    {
        public IHttpRequest Request { get; }

        public IHttpResponse Response { get; }

        public IHttpSession Session { get; }

        public HttpContext(HttpListenerRequest listenerRequest, HttpListenerResponse listenerResponse, IHttpSession session)
        {
            Request = new ListenerRequest(listenerRequest);
            Response = new ListenerResponse(listenerResponse);
            Session = session;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await Response.DisposeAsync();
            await Session.DisposeAsync();
        }
    }
}
