using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Http.HttpListener
{
    /// <inheritdoc cref="IHttpResponse"/>
    internal sealed class ListenerResponse : IHttpResponse
    {
        private readonly HttpListenerResponse _listenerResponse;

        /// <inheritdoc/>
        public int StatusCode
        {
            get => _listenerResponse.StatusCode;
            set => _listenerResponse.StatusCode = value;
        }

        /// <inheritdoc/>
        public string StatusDescription
        {
            get => _listenerResponse.StatusDescription;
            set => _listenerResponse.StatusDescription = value;
        }

        /// <inheritdoc/>
        public Stream OutputStream => _listenerResponse.OutputStream;

        public ListenerResponse(HttpListenerResponse listenerResponse)
        {
            _listenerResponse = listenerResponse;
        }

        /// <inheritdoc/>
        public void SetHeaderValue(string header, string value)
        {
            switch (header)
            {
                case "Content-Length":
                    _listenerResponse.ContentLength64 = long.Parse(value);
                    break;

                case "Content-Type":
                    _listenerResponse.ContentType = value;
                    break;

                default:
                    _listenerResponse.Headers[header] = value;
                    break;
            }
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            _listenerResponse.Close();
            return default;
        }
    }
}
