using System;
using System.IO;

namespace SecureFolderFS.Core.WebDav.Http
{
    // TODO: Needs docs
    internal interface IHttpResponse : IAsyncDisposable
    {
        int StatusCode { get; set; }

        string StatusDescription { get; set; }

        Stream OutputStream { get; }

        void SetHeaderValue(string header, string value);
    }
}
