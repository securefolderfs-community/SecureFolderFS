using SecureFolderFS.Core.WebDav.AppModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace SecureFolderFS.Core.WebDav.Http.HttpListener
{
    /// <inheritdoc cref="IHttpRequest"/>
    internal sealed class ListenerRequest : IHttpRequest
    {
        private readonly HttpListenerRequest _listenerRequest;
        private readonly NameValueCollectionToReadOnlyDictionaryBridge _collectionBridge;

        /// <inheritdoc/>
        public string HttpMethod => _listenerRequest.HttpMethod;

        /// <inheritdoc/>
        public Uri? Url => _listenerRequest.Url;

        /// <inheritdoc/>
        public Stream? InputStream => _listenerRequest.InputStream == Stream.Null ? null : _listenerRequest.InputStream;

        /// <inheritdoc/>
        public string UserAgent => _listenerRequest.UserAgent;

        /// <inheritdoc/>
        public long ContentLength => _listenerRequest.ContentLength64;

        /// <inheritdoc/>
        public Encoding ContentEncoding => _listenerRequest.ContentEncoding;

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string?> Headers => _collectionBridge;

        // TODO: Deprecated
        public IEnumerable<string?> Headers2 => _listenerRequest.Headers.AllKeys;

        // TODO: Deprecated
        public string? GetHeaderValue(string header) => _listenerRequest.Headers[header];

        public ListenerRequest(HttpListenerRequest listenerRequest)
        {
            _listenerRequest = listenerRequest;
            _collectionBridge = new(_listenerRequest.Headers);
        }
    }
}
